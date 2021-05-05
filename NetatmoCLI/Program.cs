using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NetatmoLib;
using Serilog;
using Serilog.Events;
using System.Configuration;
using System.Linq;
using System.Net;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace NetatmoCLI
{
    public class Options
    {
        [Option('l', "loop", Required = false, HelpText = "Refresh temperature data continuously in a loop.")]
        public bool Loop { get; set; }
        [Option('k',"keyvalue", Required = false, HelpText = "Machine readable key=value output.")]
        public bool KeyValue { get; set; }
    }

    

    internal class Program
    {
        /// <summary>
        ///     Exit and optionally print an error message.
        /// </summary>
        /// <param name="msg"></param>
        public static void Die(string msg = null)
        {
            if (msg != null)
                Log.Fatal($"ERROR: {msg}");
            Environment.Exit(1);
        }
        public static IConfigurationRoot Configuration { get; set; }
        private static async Task Main(string[] args)
        {
            // Set up logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File("netatmo.log", rollingInterval: RollingInterval.Day).CreateLogger();
            // TODO: Get log file path from a configuration
            Log.Debug("Logging started");
            Console.Title = "NetatmoCLI";
            
            // set up configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            var netatmoCreds = new NetatmoApiAuthCredentials();
            Configuration.GetSection("netatmo_api_auth").Bind(netatmoCreds);
            
            var netAuth = new NetatmoAuth(netatmoCreds);

            var options = new Options();
            // Parse arguments using CommandLine module
            Parser.Default.ParseArguments<Options>(args).WithParsed(o => { options = o; });

            if (options.Loop)
            {
                Log.Debug("Doing a loop");
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Updating data in a loop...");
                    Console.Out.Flush();
                    await DisplayTemp(netAuth, false);
                    Thread.Sleep(5000);
                }
            }

            await DisplayTemp(netAuth, options.KeyValue);
            Log.CloseAndFlush();
        }


        /// <summary>
        /// Display temperature of one sensor/module
        /// </summary>

        public static void DisplaySensor(string sensorName, float temperature)
        {
            Console.WriteLine($" Sensor: {sensorName}, Temperature: {temperature}");
        }
        
        public static void DisplaySensorAsKv(string stationName, string sensorName, float temperature)
        {
            Console.WriteLine($"{stationName}:{sensorName}:temperature={temperature}");
        }
        
        public static void DisplaySensorAsKv(string stationName, string sensorName, float temperature, int? CO2)
        {
            Console.WriteLine($"{stationName}:{sensorName}:temperature={temperature}");
            Console.WriteLine($"{stationName}:{sensorName}:co2={CO2}");
        }



        public static void DisplayDevice(Device device)
        {
            DisplayDevice(device, false);
        }
        
        /// <summary>
        /// Display status of one device.
        /// </summary>
        public static void DisplayDevice(Device device, bool keyvalue)
        {
            //todo: check last updated time for all modules, not just the base station
            // as it is quite possible that a module has been disconnected and this should be clearly
            // stated in such case
            var lastUpdateString = DateTimeOps.GetLastUpdateString(device.last_status_store);
            
            if (!keyvalue)
            {
                Console.WriteLine($"{device.station_name} (updated {lastUpdateString} ago)");

            }


            if (keyvalue)
            {
                DisplaySensorAsKv(device.station_name, device.module_name, (float) device.dashboard_data.Temperature, device.dashboard_data.CO2);

            }
            else
            {
                DisplaySensor(device.module_name, (float) device.dashboard_data.Temperature);

            }
            foreach (var module in device.modules)
            {
                if (module.reachable)
                {
                    if (keyvalue)
                    {
                        if (module.dashboard_data.CO2 != null)
                        {
                            DisplaySensorAsKv(device.station_name, module.module_name,
                                (float) module.dashboard_data.Temperature, module.dashboard_data.CO2);
                        }
                        else
                        {
                            DisplaySensorAsKv(device.station_name, module.module_name,
                                (float) module.dashboard_data.Temperature);
                        }

                    }
                    else
                    {
                        DisplaySensor(module.module_name, (float) module.dashboard_data.Temperature);

                    }
                }
                else
                {
                    if (keyvalue)
                    {
                        Console.WriteLine($" Sensor: {module.module_name} is unreachable.");
                    }
                }
            }
                
        }

        /// <summary>
        ///     Display temperature data on the CLI.
        /// </summary>
        public static async Task DisplayTemp(NetatmoAuth netAuth, bool keyvalue)
        {
            var netatmoDevices = await NetatmoQueries.GetTempAsync(netAuth.GetToken());
            if (keyvalue)
            {
                foreach (var device in netatmoDevices)
                    if (DateTimeOps.IsDataFresh(device.last_status_store))
                    {
                        DisplayDevice(device, keyvalue);
                    }
                 

                Console.Out.Flush();
            }
            else
            {
                Console.WriteLine(new string('-', 60));
                foreach (var device in netatmoDevices)
                    if (DateTimeOps.IsDataFresh(device.last_status_store))
                    {
                        DisplayDevice(device);
                    }
                    else
                    {
                        Console.WriteLine($"Data from device: {device.station_name} is more than 20 minutes old.");
                    }

                Console.Out.Flush();
            }
            
        }
    }
}