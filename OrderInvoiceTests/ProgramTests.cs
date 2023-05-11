using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
	[TestClass()]
	public class ProgramTests
	{
		public readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];

		[TestMethod()]
		public void MainTest()
		{
			IConfiguration config = new ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build();
			ConfigData configData = config.GetSection("ConfigData").Get<ConfigData>();
			QueueSettings queueSettings = config.GetSection("QueueSettings").Get<QueueSettings>();

			System.Threading.CancellationTokenSource source = new();
			source.CancelAfter(35000);
			System.Threading.CancellationToken token = source.Token;

			try
			{
				var t = Task.Run(() => { Program.Main(Array.Empty<string>()).GetAwaiter(); });
				t.Wait(token);
			}
			catch (Exception ex)
			{
				Assert.AreEqual(ex.Message, "The operation was canceled.");
			}
		}
	}
}