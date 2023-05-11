namespace Exito.Integracion.TurboCarulla.OrderInvoice.Classes.Settings
{
	public interface IRedisSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string Password { get; set; }
		public int DefaultExpiry { get; set; }
	}

	public class RedisSettings : IRedisSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string Password { get; set; }
		public int DefaultExpiry { get; set; }
	}
}

