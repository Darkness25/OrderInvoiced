using System.Collections.Generic;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public interface IDatabaseSettings
	{
		public MongoDatabaseType Mongo { get; set; }
	}
	public class DatabaseSettings : IDatabaseSettings
	{
		public MongoDatabaseType Mongo { get; set; }
	}

	public class MongoDatabaseType
	{
		public string ConnectionString { get; set; }
		public List<DatabaseType> Databases { get; set; }
	}

	public class DatabaseType
	{
		public string DatabaseName { get; set; }
		public string CollectionName { get; set; }
	}
}
