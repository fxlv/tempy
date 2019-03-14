using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace TempyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // initialize logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File("api.log", rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug("logging started");
            
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Call additional providers here as needed.
                    // Call AddEnvironmentVariables last if you need to allow environment
                    // variables to override values from other providers.
                    config.AddEnvironmentVariables("SETTINGS_");
                    config.AddJsonFile("appsettings.json");
                    
                })
                .UseStartup<Startup>()
                .UseUrls("http://*:5000");
        }
    }
}