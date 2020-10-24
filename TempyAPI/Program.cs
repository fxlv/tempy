using System;
using System.IO;
using System.IO.Enumeration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using TempyLogger;

namespace TempyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                TempyConfiguration.Configuration tConfiguration = new TempyConfiguration.Configuration();
                Logger.Initilize(tConfiguration);
            

                CreateWebHostBuilder(tConfiguration.ConfigurationFile).Build().Run();
                Log.Information("Web server started");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine();
                Console.WriteLine("Could not read the configuration file.");
                Console.WriteLine($"Exception: {e.Message}");
                Console.WriteLine();
                Environment.Exit(1);
            }
            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string configurationFile)
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Call additional providers here as needed.
                    // Call AddEnvironmentVariables last if you need to allow environment
                    // variables to override values from other providers.
                    config.AddEnvironmentVariables("SETTINGS_");
                    config.AddJsonFile(configurationFile);
                    
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