using Exito.Integracion.TurboCarulla.OrderInvoice.Classes;
using Exito.Integracion.TurboCarulla.OrderInvoice.Classes.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
	[TestClass()]
	public class RedisAdapterTests
	{
		private static readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];
		private static readonly IConfiguration config = new ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build();
		readonly IRedisSettings redisSettings = config.GetSection("RedisSettings").Get<RedisSettings>();

		[TestMethod()]
		public async Task UpdateCacheAsyncTest()
		{
			var mockMultiplexer = new Mock<IConnectionMultiplexer>();
			mockMultiplexer.Setup(_ => _.IsConnected).Returns(false);
			var mockDatabase = new Mock<IDatabase>();
			mockDatabase.Setup(_ => _.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);
			mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
			var logger = new Mock<ILogger<RedisAdapter>>();
			Models.OrderValidated.ResponseData requestData = JsonConvert.DeserializeObject<Models.OrderValidated.ResponseData>(System.IO.File.ReadAllText("Data/OrderValidated.json"));
			requestData.TraceId = Guid.NewGuid().ToString();
			RedisAdapter redisAdapter = new(mockMultiplexer.Object, redisSettings, logger.Object);
			Assert.IsTrue(await redisAdapter.UpdateCacheAsync(requestData));
		}

		[TestMethod()]
		public async Task UpdateCacheexceptionTest()
		{
			var mockMultiplexer = new Mock<IConnectionMultiplexer>();
			mockMultiplexer.Setup(_ => _.IsConnected).Returns(false);
			var mockDatabase = new Mock<IDatabase>();
			mockDatabase.Setup(_ => _.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);
			mockMultiplexer.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
			var logger = new Mock<ILogger<RedisAdapter>>();
			Models.OrderValidated.ResponseData requestData = JsonConvert.DeserializeObject<Models.OrderValidated.ResponseData>(System.IO.File.ReadAllText("Data/OrderValidated.json"));
			requestData.TraceId = Guid.NewGuid().ToString();
			RedisAdapter redisAdapter = new(null, redisSettings, logger.Object);
			Assert.IsFalse(await redisAdapter.UpdateCacheAsync(requestData));
		}
	}
}
