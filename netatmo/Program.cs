using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using NetatmoLib;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace NetatmoCLI
{
    public class Options
    {
        [Option('l', "loop", Required = false, HelpText = "Refresh temperature data continuously in a loop.")]
        public bool Loop { get; set; }
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

        private static async Task Main(string[] args)
        {
            // Set up logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File("netatmo.log", rollingInterval: RollingInterval.Day).CreateLogger();
            // TODO: Get log file path from a configuration
            Log.Debug("Logging started");
            Console.Title = "NetatmoCLI";
            var netAuth = new NetatmoAuth();

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
                    await DisplayTemp(netAuth);
                    Thread.Sleep(5000);
                }
            }

            await DisplayTemp(netAuth);
            Log.CloseAndFlush();
        }


        /// <summary>
        /// Display status of one device.
        /// </summary>
        public static void DisplayDevice(Device device)
        {
            //todo: check last updated time for all modules, not just the base station
            // as it is quite possible that a module has been disconnected and this should be clearly
            // stated in such case
            var lastUpdateString = DateTimeOps.GetLastUpdateString(device.last_status_store);
            Console.WriteLine($"{device.station_name} (updated {lastUpdateString} ago)");
            Console.WriteLine(
                $" Sensor: {device.module_name}, temperature: {device.dashboard_data.Temperature}");
            foreach (var module in device.modules)
                Console.WriteLine(
                    $" Sensor: {module.module_name}, Temperature: {module.dashboard_data.Temperature}");
        }

        /// <summary>
        ///     Display temperature data on the CLI.
        /// </summary>
        public static async Task DisplayTemp(NetatmoAuth netAuth)
        {
            var netatmoDevices = await NetatmoQueries.GetTempAsync(netAuth.GetToken());

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