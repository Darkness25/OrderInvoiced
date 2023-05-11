using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public static class DataTracker
	{
		public static readonly Queue<IDictionary<string, object>> DataQueue = new();
		public static ILogger Logger { get; set; }

		public static async Task TrackEventAsync(object headers, object body, string operation, string traceId, Exception exception)
		{
			IDictionary<string, object> dataTrack = new Dictionary<string, object>
			{
				{ "Headers", headers },
				{ "Body", body },
				{ "Operation", operation },
				{ "TraceId", traceId },
				{ "Exception", exception },
				{ "Timestamp", DateTime.Now }
			};

			int counter = 0;

			do
				try
				{
					lock (DataQueue) DataQueue.Enqueue(dataTrack);
					break;
				}
				catch (Exception ex)
				{
					Logger.LogError("[OrderInvoice] Data queued failed: {ex.Message} Try: {counter}", ex.Message, counter);
					counter++;
					Thread.Sleep(100);
				}
			while (counter < 3);

			await Task.Delay(1);
		}
	}

	public class DataTrackerHostedService : BackgroundService
	{
		private readonly IElkSettings elkSettings;
		private bool Opened;
		private readonly Exito.Integracion.Trazabilidad.Logger logger;
		private readonly System.Timers.Timer aTimer;

		public DataTrackerHostedService(IConfiguration config)
		{
			this.elkSettings = config.GetSection("ElkSettings").Get<ElkSettings>();
			logger = new(elkSettings.Tracking.Mode, elkSettings.Tracking.Host, elkSettings.Tracking.Username, elkSettings.Tracking.VirtualHost, elkSettings.Tracking.Password, elkSettings.Tracking.Protocol, elkSettings.Tracking.Port, elkSettings.Tracking.ServiceName, elkSettings.Tracking.ConnTimeout, elkSettings.Tracking.ReadTimeout, true);
			Opened = elkSettings.Tracking.Enabled && logger.OpenConnection();
			aTimer = new(30000);
			aTimer.Elapsed += OnTimedEvent; aTimer.AutoReset = true; aTimer.Enabled = true;
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await DoWork(cancellationToken);
		}

		private async Task DoWork(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					if (DataTracker.DataQueue.Count > 0)
					{
						IDictionary<string, object> dataTrack = DataTracker.DataQueue.Dequeue();

						if (dataTrack != null)
						{
							int counter = 0;
							do
							{
								if (await ElkTrackingAsync(dataTrack["Headers"], dataTrack["Body"], dataTrack["Operation"].ToString(), (DateTime)dataTrack["Timestamp"], dataTrack["TraceId"].ToString(), (Exception)dataTrack["Exception"])) break;
								else
								{
									counter++;
									Thread.Sleep(10);
								}
							} while (counter < 3);
						}
					}
					else { Thread.Sleep(2); if (DataTracker.DataQueue.Count == 0) { lock (DataTracker.DataQueue) DataTracker.DataQueue.TrimExcess(); } }
				}
				catch (Exception ex)
				{
					DataTracker.Logger.LogError("[OrderInvoice] Data model failed: {ex.Message}", ex.Message);
					DataTracker.Logger.LogError("[OrderInvoice] {ex.Message} {ex.GetHashCode().ToString()}", ex.Message, ex.GetHashCode().ToString());
				}
			}
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);
		}

		private async Task<bool> ElkTrackingAsync(object headers, object body, string operation, DateTime timestamp, string traceId, Exception exception)
		{
			bool response = true;

			if (elkSettings.Tracking.Enabled)
			{
				Exito.Integracion.Trazabilidad.Models.LogData log = new()
				{
					TransactionID = traceId,
					IntegrationName = elkSettings.Tracking.ServiceName,
					DomainName = elkSettings.Tracking.NameSpace,
					Operation = operation,
					Type = operation.Contains("/request:") ? "IN" : "OUT",
					TimeStamp = timestamp,
					Event = new Exito.Integracion.Trazabilidad.Models.EventType
					{
						Header = new Exito.Integracion.Trazabilidad.Models.HeaderType
						{
							TransactionID = traceId,
							ApplicationID = elkSettings.Tracking.ServiceName,
							TransactionDate = timestamp,
							FlexField = headers
						},
						Data = body
					},
					Status = exception != null ? "ERROR" : "OK",
					Trace = exception != null ? exception.Message : "Success",
					MessageResult = exception != null ? exception.StackTrace : "Success"
				};

				try
				{
					if (!Opened) Opened = logger.OpenConnection();
					if (Opened)
					{
						aTimer.Interval = 30000;
						response = await logger.SendLogAsync(elkSettings.Tracking.ExchangeName, elkSettings.Tracking.RoutingKey, log, (exception == null) ? 1 : 0);
					}
					else response = false;
				}
				catch { response = false; }
			}

			return response;
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			if (Opened) logger.CloseConnection();
			Opened = false;
		}
	}
}