using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

using TempyWorker;

namespace TempyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TempyConfiguration tConfiguration = new TempyConfiguration();
            TempyLogger.Initilize(tConfiguration);
            
            CreateWebHostBuilder(args).Build().Run();
            Log.Information("Web server started");
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
        
        // TODO: move this somewhere else, not sure where yet.
        public static void LogHttpRequest(string method, string statusCode, string remoteIp, string requestPath)
        {
            Log.Debug($"{method} {statusCode} {remoteIp} {requestPath}");

        }
    }
}