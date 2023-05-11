namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public interface IElkSettings
	{
		public TrackingType Tracking { get; set; }
	}

	public class ElkSettings : IElkSettings
	{
		public TrackingType Tracking { get; set; }
	}

	public class TrackingType
	{
		public bool Enabled { get; set; }
		public string Mode { get; set; }
		public string Host { get; set; }
		public string VirtualHost { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Protocol { get; set; }
		public int Port { get; set; }
		public string ExchangeName { get; set; }
		public string RoutingKey { get; set; }
		public string NameSpace { get; set; }
		public string ServiceName { get; set; }
		public int ConnTimeout { get; set; }
		public int ReadTimeout { get; set; }
	}
}
