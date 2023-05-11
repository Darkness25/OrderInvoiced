
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			IHostBuilder builder = new HostBuilder()
				 .ConfigureAppConfiguration((hostingContext, config) =>
				 {
					 if (File.Exists("settings/appsettings.json"))
						 config.AddJsonFile("settings/appsettings.json", optional: true, reloadOnChange: true);
					 else
						 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
					 config.AddEnvironmentVariables();

					 if (args != null) config.AddCommandLine(args);
				 })
				 .ConfigureLogging(logging =>
				 {
					 System.Environment.SetEnvironmentVariable("Log4NetFilename", System.Net.Dns.GetHostName());
					 logging.ClearProviders();
					 logging.SetMinimumLevel(LogLevel.Trace);
					 logging.AddLog4Net("Logger.config");
				 })
				 .ConfigureServices((hostContext, services) =>
				 {
					 services.AddOptions();

					 services.AddHostedService<QueueServiceHostedService>();
					 services.AddHostedService<DataTrackerHostedService>();
				 })
				 ;
			await builder.RunConsoleAsync();
		}
	}
}
