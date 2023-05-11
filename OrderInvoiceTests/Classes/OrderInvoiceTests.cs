using Exito.Integracion.TurboCarulla.OrderInvoice.Classes;
using Exito.Integracion.TurboCarulla.OrderInvoice.Classes.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
    [TestClass()]
    public class OrderInvoiceTests
    {

        static readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
        static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build();
        readonly IConfigData configData = config.GetSection("ConfigData").Get<ConfigData>();
        readonly QueueSettings queueSettings = config.GetSection("QueueSettings").Get<QueueSettings>();
        //readonly IDatabaseSettings databaseSettings = config.GetSection("DatabaseSettings").Get<DatabaseSettings>();
        //readonly IRedisSettings redisSettings = config.GetSection("RedisSettings").Get<RedisSettings>();
        readonly RedisSettings redisSettings = new RedisSettings()
        {
            DefaultExpiry = 100,
            Host = "lOCALHOST",
            Password = "Tests",
            Port = 6710
        };

        readonly DatabaseSettings databaseSettings = new DatabaseSettings()
        {
            Mongo = new MongoDatabaseType
            {
                ConnectionString = "Tests",
                Databases = new List<DatabaseType>
                   {
                       new DatabaseType
                       {
                           CollectionName = "TEst",
                           DatabaseName = "Tests"
                       }

                   }
            }
        };

        [TestMethod()]
        public void DequeueAsyncTest()
        {

            var configDataOption = new Mock<IOptionsSnapshot<ConfigData>>();
            configDataOption.Setup(m => m.Value).Returns((ConfigData)configData);
            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            mockMultiplexer.Setup(_ => _.IsConnected).Returns(false);

            var dataSuccess = new List<Models.Pim.Maestras.ResponseData>
            {
                new Models.Pim.Maestras.ResponseData
                {
                    Description = "Tests",
                    LocationId = "4842",
                    Address = "tests",
                    City = "Tests",
                    CityName = "Tests",
                    DaneCode = "01",
                    Phone = "88"
                }

            };
            var _mockCursor = new Mock<IAsyncCursor<Models.Pim.Maestras.ResponseData>>();
            _mockCursor.Setup(_ => _.Current)
                .Returns(dataSuccess);
            _mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            var _collectionMock = new Mock<IMongoCollection<Models.Pim.Maestras.ResponseData>>();
            _collectionMock
                .Setup(_ => _.FindAsync(It.IsAny<FilterDefinition<Models.Pim.Maestras.ResponseData>>(),
                                        It.IsAny<FindOptions<Models.Pim.Maestras.ResponseData, Models.Pim.Maestras.ResponseData>>(),
                                        It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(_mockCursor.Object);

            var mockMongo = new Mock<IMongoClient>();

            var _databaseMock = new Mock<IMongoDatabase>();
            mockMongo.Setup(x => x.GetDatabase(It.IsAny<string>(),
                                                      It.IsAny<MongoDatabaseSettings>()))
                                                      .Returns(_databaseMock.Object);
            _databaseMock.Setup(x => x.GetCollection<Models.Pim.Maestras.ResponseData>(It.IsAny<string>(),
                                                                             It.IsAny<MongoCollectionSettings>())).Returns(_collectionMock.Object);


            //var mockCollection = new Mock<IMongoCollection<Models.Pim.Maestras.ResponseData>>();
            var mockCollectionSap = new Mock<IMongoCollection<Models.Sap.InvoicePending.RequestData>>();
            var mockCollectionPoblaciones = new Mock<IMongoCollection<Models.Sap.Poblaciones.ResponseData>>();








            //mockMongo.Setup(x => x.GetDatabase(It.IsAny<string>(),
            //                                          It.IsAny<MongoDatabaseSettings>()))
            //                                          .Returns(_databaseMock.Object);
            //_databaseMock.Setup(x => x.GetCollection<Models.Pim.Maestras.ResponseData>(It.IsAny<string>(),
            //                                                                 It.IsAny<MongoCollectionSettings>())).Returns(mockCollection.Object);
            //_databaseMock.Setup(x => x.GetCollection<Models.Sap.InvoicePending.RequestData>(It.IsAny<string>(),
            //                                                                 It.IsAny<MongoCollectionSettings>())).Returns(mockCollectionSap.Object);
            //_databaseMock.Setup(x => x.GetCollection<Models.Sap.Poblaciones.ResponseData>(It.IsAny<string>(),
            //                                                                 It.IsAny<MongoCollectionSettings>())).Returns(mockCollectionPoblaciones.Object);




            var logger = new Mock<ILogger>();

            var orderValidatedQueue = new QueueAdapter(queueSettings, queueSettings.Queues[1], logger.Object);
            var orderDiscartQueue = new QueueAdapter(queueSettings, queueSettings.Queues[2], logger.Object);

            Models.OrderValidated.ResponseData requestData = JsonConvert.DeserializeObject<Models.OrderValidated.ResponseData>(System.IO.File.ReadAllText("Data/OrderValidated.json"));
            requestData.TraceId = Guid.NewGuid().ToString();

            //MongoDB.Driver.MongoClient mongoClient = new(databaseSettings.Mongo.ConnectionString);
            OrderInvoice orderInvoice = new(configData, queueSettings, databaseSettings, mockMongo.Object, mockMultiplexer.Object, redisSettings, logger.Object);
            bool response = orderInvoice.SendToService(requestData).Result;
            Assert.IsTrue(response);
        }
    }

    //[TestClass()]
    //public class OrderInvoiceTests
    //{
    //    static readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
    //    static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build();
    //    readonly IConfigData configData = config.GetSection("ConfigData").Get<ConfigData>();
    //    readonly QueueSettings queueSettings = config.GetSection("QueueSettings").Get<QueueSettings>();
    //    readonly IDatabaseSettings databaseSettings = config.GetSection("DatabaseSettings").Get<DatabaseSettings>();
    //    readonly IRedisSettings redisSettings = config.GetSection("RedisSettings").Get<RedisSettings>();

    //    [TestMethod()]
    //    public void DequeueAsyncTest()
    //    {
    //        var configDataOption = new Mock<IOptionsSnapshot<ConfigData>>();
    //        configDataOption.Setup(m => m.Value).Returns((ConfigData)configData);
    //        var mockMultiplexer = new Mock<IConnectionMultiplexer>();
    //        mockMultiplexer.Setup(_ => _.IsConnected).Returns(false);

    //        var logger = new Mock<ILogger>();

    //        var orderValidatedQueue = new QueueAdapter(queueSettings, queueSettings.Queues[1], logger.Object);
    //        var orderDiscartQueue = new QueueAdapter(queueSettings, queueSettings.Queues[2], logger.Object);

    //        Models.OrderValidated.ResponseData requestData = JsonConvert.DeserializeObject<Models.OrderValidated.ResponseData>(System.IO.File.ReadAllText("Data/OrderValidated.json"));
    //        requestData.TraceId = Guid.NewGuid().ToString();

    //        MongoDB.Driver.MongoClient mongoClient = new(databaseSettings.Mongo.ConnectionString);
    //        OrderInvoice orderInvoice = new(configData, queueSettings, databaseSettings, mongoClient, mockMultiplexer.Object, redisSettings, logger.Object);
    //        bool response = orderInvoice.SendToService(requestData).Result;
    //        Assert.IsTrue(response);
    //    }
    //}
}