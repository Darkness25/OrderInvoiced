using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Classes
{
    public interface IQueueAdapter
    {
        IModel OpenConnection();
        void CloseConnection();
        IModel GetChannel();
        QueueType GetQueue();
        Task<bool> QueueMessageAsync(string message);
        Task<bool> QueueMessageAsync(string exchangeName, string routingKey, string message, int priority);
    }

    public class QueueAdapter : IQueueAdapter
    {
        private IConnection conn;
        private IModel channel;

        private readonly QueueSettings queueSettings;
        private readonly QueueType queue;
        private readonly ILogger logger;

        public QueueAdapter(QueueSettings queueSettings, QueueType queue, ILogger logger)
        {
            this.queueSettings = queueSettings;
            this.queue = queue;
            this.logger = logger;
        }

        public IModel OpenConnection()
        {
            int maxAttempts = 3;
            int currentAttempt = 1;

            CancellationTokenSource cts = new CancellationTokenSource(queueSettings.ConnTimeout);

            while (currentAttempt <= maxAttempts)
            {
                try
                {
                    if (channel != null && channel.IsOpen) return channel;

                    ConnectionFactory rabbitConnFactory = new()
                    {
                        HostName = queueSettings.Host,
                        VirtualHost = queueSettings.VirtualHost,
                        UserName = queueSettings.Username,
                        Password = queueSettings.Password,
                        Port = queueSettings.Port,

                        Ssl = new SslOption
                        {
                            ServerName = queueSettings.Host,
                            Enabled = queueSettings.Protocol.ToUpper().Equals("AMQPS") || queueSettings.Protocol.ToUpper().Equals("HTTPS"),
                            Version = System.Security.Authentication.SslProtocols.Tls12,
                            AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNotAvailable
                        },
                        ContinuationTimeout = TimeSpan.FromMilliseconds(queueSettings.ReadTimeout),
                        RequestedConnectionTimeout = TimeSpan.FromMilliseconds(queueSettings.ConnTimeout)
                    };

                    conn = rabbitConnFactory.CreateConnection(queueSettings.ServiceName);
                    channel = conn.CreateModel();

                    if (queue.ExchangeName.ToLower() is "domainevents") { }
                    else
                    {
                        channel.BasicQos(0, queueSettings.PrefetchCount, false);
                        Dictionary<string, object> queueArgs = new() { { "x-queue-type", "classic" } };
                        channel.ExchangeDeclare(exchange: queue.ExchangeName, type: "direct", durable: true);
                        channel.QueueDeclare(queue: queue.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
                        channel.QueueBind(queue: queue.QueueName, exchange: queue.ExchangeName, routingKey: queue.RoutingKey);
                        string exchangeRetry = queue.ExchangeName + "-retry";
                        string queueRetry = queue.QueueName + "-retry";
                        string routinKeyRetry = queue.RoutingKey + "-retry";
                        queueArgs = new()
                    {
                        { "x-queue-type", "classic" },
                        { "x-dead-letter-exchange", queue.ExchangeName },
                        { "x-dead-letter-routing-key", queue.RoutingKey },
                        { "x-message-ttl", queueSettings.RetryInterval }
                    };
                        channel.ExchangeDeclare(exchange: exchangeRetry, type: "direct", durable: true);
                        channel.QueueDeclare(queue: queueRetry, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
                        channel.QueueBind(queue: queueRetry, exchange: exchangeRetry, routingKey: routinKeyRetry);
                    }

                    logger.LogInformation("[OrderInvoice] Connection to RabbitMQ successfully: host={queueSettings.Host} - queue={queue.QueueName}", queueSettings.Host, queue.QueueName);
                    return channel;
                }
                catch (OperationCanceledException)
                {
                    logger.LogError("[OrderInvoice] Connection queue timed out: host={queueSettings.Host} - Attempt={currentAttempt}", queueSettings.Host, currentAttempt);
                    cts.Dispose();
                    currentAttempt++;
                    cts = new CancellationTokenSource(queueSettings.ConnTimeout);
                    return null;    
                }
                catch (Exception ex)
                {
                    logger.LogError("[OrderInvoice] Connection queue failed: host={queueSettings.Host} - Error= {ex.Message}", queueSettings.Host, ex.Message);                    
                    return null;

                }
            }
            return null;
        }

        public void CloseConnection()
        {
            if (!channel.IsClosed)
            {
                channel.Close();
                conn.Close();
            }
        }

        public IModel GetChannel() { return channel; }
        public QueueType GetQueue() { return queue; }

        public async Task<bool> QueueMessageAsync(string message)
        {

            return await QueueMessageAsync(queue.ExchangeName, queue.RoutingKey, message, 0);
        }

        public async Task<bool> QueueMessageAsync(string exchangeName, string routingKey, string message, int priority)
        {
            if (channel.IsOpen)
            {
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Priority = (byte)priority;
                await Task.Run(() => channel.BasicPublish(exchangeName, routingKey, properties, messageBodyBytes));
                return true;
            }
            else return false;
        }
    }
}
