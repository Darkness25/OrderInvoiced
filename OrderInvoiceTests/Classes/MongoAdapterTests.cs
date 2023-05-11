using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
	[TestClass()]
	public class MongoAdapterTests
	{
		private static readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
		private static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build();
		readonly ConfigData configData = config.GetSection("ConfigData").Get<ConfigData>();
		readonly IDatabaseSettings databaseSettings = config.GetSection("DatabaseSettings").Get<DatabaseSettings>();

		[TestMethod()]
		public async Task GetLocationDataAsyncTest()
		{
			var configDataOption = new Mock<IOptionsSnapshot<ConfigData>>();
			configDataOption.Setup(m => m.Value).Returns(configData);
			var logger = new Mock<ILogger>();

			MongoDB.Driver.MongoClient mongoClient = new(databaseSettings.Mongo.ConnectionString);

			MongoAdapter mongoAdapter = new(databaseSettings, mongoClient, logger.Object);

			Models.Pim.Maestras.RequestData requestData = new()
			{
				LocationId = 4842,
				TraceId = Guid.NewGuid().ToString()
			};

			Models.Pim.Maestras.ResponseData responseData = await mongoAdapter.GetLocationDataAsync(requestData);

			Assert.IsNotNull(responseData);
			Assert.AreEqual(requestData.TraceId, responseData.TraceId);

			requestData.LocationId = -1;
			responseData = await mongoAdapter.GetLocationDataAsync(requestData);

			Assert.IsNull(responseData);
		}

		[TestMethod()]
		public async Task GetPoblacionesDataAsyncTest()
		{
			var configDataOption = new Mock<IOptionsSnapshot<ConfigData>>();
			configDataOption.Setup(m => m.Value).Returns(configData);
			var logger = new Mock<ILogger>();

			MongoDB.Driver.MongoClient mongoClient = new(databaseSettings.Mongo.ConnectionString);

			MongoAdapter mongoAdapter = new(databaseSettings, mongoClient, logger.Object);

			Models.Sap.Poblaciones.RequestData requestData = new()
			{
				DaneCode = 5266,
				TraceId = Guid.NewGuid().ToString()
			};

			Models.Sap.Poblaciones.ResponseData responseData = await mongoAdapter.GetPoblacionesDataAsync(requestData);

			Assert.IsNotNull(responseData);
			Assert.AreEqual(requestData.TraceId, responseData.TraceId);

			requestData.DaneCode = 0;
			responseData = await mongoAdapter.GetPoblacionesDataAsync(requestData);

			Assert.IsNull(responseData);
		}

		[TestMethod()]
		public async Task InsertInvoiceAsyncTest()
		{
			var configDataOption = new Mock<IOptionsSnapshot<ConfigData>>();
			configDataOption.Setup(m => m.Value).Returns(configData);
			var logger = new Mock<ILogger>();

			MongoDB.Driver.MongoClient mongoClient = new(databaseSettings.Mongo.ConnectionString);

			MongoAdapter mongoAdapter = new(databaseSettings, mongoClient, logger.Object);

			Models.Sap.InvoicePending.RequestData requestData = new()
			{
				Id = MongoDB.Bson.ObjectId.Parse(Tools.PadLeftData("1234", 24)),
				Burks = configData.SapSettings.Burks,
				Business = configData.SapSettings.Business,
				Country = configData.SapSettings.Country,
				Customer = new Models.Sap.InvoicePending.CustomerType
				{
					Address = "Dirección Prueba",
					City = "Envigado",
					DocumentNumber = "999993",
					DocumentType = 2,
					Email = "email@server.com",
					Name = "Usuario Pruena",
					PhoneNumber = "1234559",
					Region = 0
				},
				Freight = configData.SapSettings.Freight,
				Index = configData.SapSettings.Index,
				Insurance = configData.SapSettings.Insurance,
				InvoiceRetry = 1,
				LocationId = "4842",
				OrderDate = DateTime.Now.ToString("yyyy-MM-dd"),
				OrderId = "1234",
				OrderPrefix = "9702",
				OrderTime = DateTime.Now.ToString("HH:mm:ss"),
				OthersCost = configData.SapSettings.OtherCost,
				PersonType = String.Empty,
				Products = new Models.Sap.InvoicePending.ProductType[]
				{
			new Models.Sap.InvoicePending.ProductType
			{
			Description = "Plu pruebas",
			Discout = "0",
			IvaRate = 16,
			MaterialCode = "0",
			Plu = "1234",
			PosNumber = 1,
			Price = "0",
			Quantity = 0,
			Sequence = 0,
			Sign = "0",
			SubLine = 0
			}
				},
				SincoId = configData.SapSettings.SincoId,
				Status = configData.SapSettings.InvoiceStatus,
				Origin = configData.SapSettings.Origin,
				TraceId = Guid.NewGuid().ToString(),
				Transaccion = new Models.Sap.InvoicePending.TransactionType
				{
					Id = "1234",
					Auth = "1234",
					Bin = 0,
					Dues = configData.SapSettings.Dues,
					PaymentMethod = configData.SapSettings.PaymentMethod,
					Points = 0,
					Value = 10
				},
				TypeBus = configData.SapSettings.TypeBus,
				Vstel = configData.SapSettings.Vstel.ToString(),
				Waers = configData.SapSettings.Waers
			};

			Models.Sap.InvoicePending.ResponseData responseData = await mongoAdapter.InsertInvoiceAsyc(requestData);

			Assert.IsNotNull(responseData);
			Assert.AreEqual(requestData.TraceId, responseData.TraceId);
		}
	}
}