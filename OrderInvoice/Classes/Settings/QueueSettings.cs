namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public class QueueSettings
	{
		public string Host { get; set; }
		public string VirtualHost { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Protocol { get; set; }
		public int Port { get; set; }
		public string ServiceName { get; set; }
		public int ConnTimeout { get; set; }
		public int ReadTimeout { get; set; }
		public ushort PrefetchCount { get; set; }
		public int RetryInterval { get; set; }
		public double RetryTime { get; set; }
		public QueueType[] Queues { get; set; }
	}

	public class QueueType
	{
		public string ExchangeName { get; set; }
		public string RoutingKey { get; set; }
		public string QueueName { get; set; }
	}
}
