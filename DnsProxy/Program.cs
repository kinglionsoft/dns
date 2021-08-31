using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DnsProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "config");
                    builder.SetBasePath(path)
                        .AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        ;
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddSystemdConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    });
                })
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.Configure<DnsOptions>(configuration.GetSection("Dns"));

                    services.AddHostedService<DnsWorker>();
                });
    }
}
