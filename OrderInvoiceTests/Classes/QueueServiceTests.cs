using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Text;
using Exito.Integracion.TurboCarulla.OrderInvoice.Classes;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
    //[TestClass()]
    //public class QueueServiceTests
    //{
    //    Mock<ILogger<QueueServiceHostedService>> _logger = new Mock<ILogger<QueueServiceHostedService>>();
    //    Mock<IConfiguration> _config = new Mock<IConfiguration>();
    //    Mock<IConfigData> _configData = new Mock<IConfigData>();

    //    [TestMethod()]
    //    public void QueueHostedServiceConstructorTest()
    //    {
    //        var orderValidateQueue = new QueueServiceHostedService(_config.Object, _logger.Object);
    //        Assert.IsNotNull(orderValidateQueue);
    //    }

    //    [TestMethod()]
    //    public async Task OrderCollectAsync()
    //    {
    //        QueueServiceHostedService queueServiceHostedService = new(_config.Object, _logger.Object);

    //        var source = new CancellationTokenSource();
    //        var token = source.Token;
    //        await queueServiceHostedService.StartAsync(token);

    //        var requestData = new Models.OrderValidated.ResponseData();
    //        var requestJson = JsonConvert.SerializeObject(requestData);
    //        var messageBody = Encoding.UTF8.GetBytes(requestJson);
    //        var message = new Message().ToString();

    //        var queueClientMock = new Mock<IQueueAdapter>();
    //        queueClientMock.Setup(x => x.QueueMessageAsync(message)).Returns(Task.CompletedTask);

    //        QueueService queueService = new(queueClientMock.Object, _configData.Object, _logger.Object);
    //        await queueService.o(requestData);

    //        await Task.Delay(5000);

    //        await queueServiceHostedService.StopAsync(CancellationToken.None);

    //        Assert.IsTrue(true);
    //    }
    //}
}