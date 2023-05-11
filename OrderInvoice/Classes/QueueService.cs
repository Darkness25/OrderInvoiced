using Exito.Integracion.TurboCarulla.OrderInvoice.Classes;
using Exito.Integracion.TurboCarulla.OrderInvoice.Classes.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public class QueueService
	{
		public static readonly Queue<Models.Sap.InvoiceCollect.RequestData> OrderCollectQueue = new();
		public static readonly Queue<Models.OrderValidated.ResponseData> OrderValidatedQueue = new();
		public static readonly ILogger<QueueService> Logger;

		public static async Task OrderValidatedAsync(Models.OrderValidated.ResponseData requestData)
		{
			int counter = 0;

			do
				try
				{
					lock (OrderValidatedQueue) OrderValidatedQueue.Enqueue(requestData);
					break;
				}
				catch (Exception ex)
				{
					Logger.LogError("[OrderInvoice] Order queued failed {ex.Message} try: {counter}", ex.Message, counter);
					counter++;
					Thread.Sleep(100);
				}
			while (counter < 3);

			await Task.Delay(1);
		}
	}

	public class QueueServiceHostedService : BackgroundService
	{
		private readonly QueueSettings queueSettings;
		private readonly QueueType queue;
		private readonly IQueueAdapter queueAdapter;
		private readonly IConfigData configData;
		private readonly IDatabaseSettings databaseSettings;
		private readonly IMongoClient mongoClient;
		private readonly IModel channel;
		private readonly IRedisSettings redisSettings;
		private readonly ConfigurationOptions configurationOptions;
		private readonly ElkSettings elkSettings;
		private readonly IConnectionMultiplexer multiplexer;
		private readonly ILogger<QueueServiceHostedService> logger;

		public QueueServiceHostedService(IConfiguration configuration, ILogger<QueueServiceHostedService> logger)
		{
			this.queueSettings = configuration.GetSection("QueueSettings").Get<QueueSettings>();
			this.queue = queueSettings.Queues[0];
			this.queueAdapter = new QueueAdapter(queueSettings, this.queue, logger);
			this.configData = configuration.GetSection("ConfigData").Get<ConfigData>();
			this.databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
			this.mongoClient = new MongoClient(databaseSettings.Mongo.ConnectionString);
			this.channel = queueAdapter.OpenConnection();
			this.redisSettings = configuration.GetSection("RedisSettings").Get<RedisSettings>();
			this.elkSettings = configuration.GetSection("ElkSettings").Get<ElkSettings>();
			this.configurationOptions = ConfigurationOptions.Parse($"{redisSettings.Host}:{redisSettings.Port}");
			configurationOptions.Password = redisSettings.Password;
			configurationOptions.ClientName = elkSettings.Tracking.ServiceName;
			this.multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
			this.logger = logger;

			if (channel.IsClosed) throw new ArgumentNullException(nameof(QueueServiceHostedService), "Cannot open connection with queues server");
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			new Task(() => { QueueWork(cancellationToken).GetAwaiter(); }).Start();
			new Task(() => { DeQueueWork(cancellationToken).GetAwaiter(); }).Start();
			await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
		}

		private async Task QueueWork(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					if (QueueService.OrderCollectQueue.Count > 0)
					{
						Models.Sap.InvoiceCollect.RequestData message = QueueService.OrderCollectQueue.Dequeue();

						if (message != null)
						{
							string exchangeName = string.Empty; string routingKey = string.Empty;

							if (message.RetryTimes == 0)
							{
								exchangeName = queue.ExchangeName;
								routingKey = queue.RoutingKey;
							}
							else
							{
								exchangeName = queue.ExchangeName + "-retry";
								routingKey = queue.RoutingKey + "-retry";
							}

							if (message.TransactionDateTime.AddHours(queueSettings.RetryTime) > DateTime.Now)
							{
								await DataTracker.TrackEventAsync(null, message, "OrderInvoice/Dequeue/request: Retry=" + message.RetryTimes, message.TraceId, null);
								await queueAdapter.QueueMessageAsync(exchangeName, routingKey, JsonConvert.SerializeObject(message), 0);
							}
							else
							{
								await DataTracker.TrackEventAsync(null, message, "OrderInvoice/Dequeue/response: Mensaje Descartado", message.TraceId, null);
							}
						}
					}
					else { Thread.Sleep(2); if (QueueService.OrderCollectQueue.Count == 0) { lock (QueueService.OrderCollectQueue) QueueService.OrderCollectQueue.TrimExcess(); } }
				}
				catch (Exception ex)
				{
					logger.LogError("[OrderInvoice] Queue update failed {ex.Message}", ex.Message);
					logger.LogError("[OrderInvoice] {ex.Message} {ex.GetHashCode().ToString()}", ex.Message, ex.GetHashCode().ToString());
				}
			}

		}

		private async Task DeQueueWork(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IOrderInvoice orderInvoice = new OrderInvoice(configData, queueSettings, databaseSettings, mongoClient, multiplexer, redisSettings, logger);
			EventingBasicConsumer eventReceiver = new(channel);
			string queueMessage = string.Empty;

			eventReceiver.Received += async (ch, ea) =>
			{
				try
				{
					queueMessage = Encoding.UTF8.GetString(ea.Body.ToArray());
					Models.OrderValidated.ResponseData message = JsonConvert.DeserializeObject<Models.OrderValidated.ResponseData>(queueMessage);

					message.RetryTimes = 0;
					message.TransactionDateTime = DateTime.Now;

					if (message != null)
					{
						if (message.TransactionDateTime.Year == 1) message.TransactionDateTime = DateTime.Now;
						await DataTracker.TrackEventAsync(null, message, "OrderInvoice/Dequeue/response: " + ea.ConsumerTag, message.TraceId, null);


						bool resultCode = await orderInvoice.SendToService(message);

						if (!resultCode)
						{
							message.RetryTimes++;
							await QueueService.OrderValidatedAsync(message);
						}

					}
				}
				catch (Exception ex)
				{
					logger.LogError("[OrderInvoice] Error:  {ex.Message}", ex.Message);
					logger.LogError("[OrderInvoice] Message received: {queueMessage}", queueMessage);
				}

				channel.BasicAck(ea.DeliveryTag, false);
			};
			channel.BasicConsume(queue.QueueName, false, eventReceiver);
			await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			queueAdapter.CloseConnection();
			await base.StopAsync(cancellationToken);
		}
	}
}
