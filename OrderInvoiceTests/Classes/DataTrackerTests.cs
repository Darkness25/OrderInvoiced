using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
	[TestClass()]
	public class DataTrackerTests
	{
		private readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];

		[TestMethod()]
		public void TrackEventTest()
		{
			Models.OrderValidated.ResponseData requestData = new()
			{
				Response = true,
				Message = "Success",
				MessageDetail = "Transaction Test",
				TraceId = Guid.NewGuid().ToString()
			};

			DataTracker.TrackEventAsync("Prueba", requestData, "OrderValidate/Dequeue/request: ", requestData.TraceId, null).GetAwaiter();
			Assert.IsTrue(true);
		}

		[TestMethod()]
		public async Task TrackEventBackTestAsync()
		{
			IConfiguration config = new ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build();
			DataTrackerHostedService dataTrackerHostedService = new(config);

			System.Threading.CancellationTokenSource source = new();
			source.CancelAfter(35000);
			System.Threading.CancellationToken token = source.Token;

			await dataTrackerHostedService.StartAsync(token);
			dataTrackerHostedService.StopAsync(new System.Threading.CancellationToken()).Wait();
			Assert.IsTrue(true);
		}
	}
}