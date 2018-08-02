using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;

namespace netatmo
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Set up logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(LogEventLevel.Information)
                .WriteTo.File("netatmo.log", rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug("Logging started");
            var netAuth = new NetatmoAuth();
            await DisplayTemp(netAuth);
            Log.CloseAndFlush();
        }

        /// <summary>
        ///     Determine if the data is fresh by looking at timestamps
        ///     Fresh here means, not older than 20 minutes.
        /// </summary>
        private static bool isDataFresh(int unixTimestamp)
        {
            var maxAllowedDiff = 1200; // 20 minutes
            var timeStamp = GetDateTimeOffset(unixTimestamp);
            var diff = GetTimeDelta(timeStamp);
            if (diff.TotalSeconds < maxAllowedDiff) return true;
            return false;
        }

        /// <summary>
        ///     Convert unixTimestamp (integer) to DateTimeOffset
        /// </summary>
        private static DateTimeOffset GetDateTimeOffset(int unixTimestamp)
        {
            var timeStamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return timeStamp;
        }

        /// <summary>
        ///     Calculate the difference between "now" and the provided timestamp
        /// </summary>
        private static TimeSpan GetTimeDelta(DateTimeOffset timeStamp)
        {
            var now = DateTime.Now;
            var diff = now - timeStamp;
            return diff;
        }

        /// <summary>
        ///     Return a human readable string, such as "5 minutes" or "1 minute" etc.
        ///     based on the unix timestamp
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns>String representation of last update time</returns>
        private static string GetLastUpdateString(int unixTimestamp)
        {
            var timeStamp = GetDateTimeOffset(unixTimestamp);
            var delta = GetTimeDelta(timeStamp);
            var humanReadableString = "unknown";
            var deltaMinutes = Math.Round(delta.TotalMinutes);
            if (delta.TotalSeconds < 60) humanReadableString = "less than a minute";
            else if (deltaMinutes == 1) humanReadableString = "a minute";
            else if (deltaMinutes > 1) humanReadableString = $"{deltaMinutes} minutes";
            return humanReadableString;
        }

        /// <summary>
        ///     Display temperature data on the CLI.
        /// </summary>
        private static async Task DisplayTemp(NetatmoAuth netAuth)
        {
            var netatmoDevices = await GetTempAsync(netAuth.GetToken());

            Console.WriteLine(new string('-', 60));
            foreach (var device in netatmoDevices)
                if (isDataFresh(device.last_status_store))
                {
                    var lastUpdateString = GetLastUpdateString(device.last_status_store);
                    Console.WriteLine($"{device.station_name} (updated {lastUpdateString} ago)");
                    Console.WriteLine(
                        $" Sensor: {device.module_name}, temperature: {device.dashboard_data.Temperature}");
                    foreach (var module in device.modules)
                        Console.WriteLine(
                            $" Sensor: {module.module_name}, Temperature: {module.dashboard_data.Temperature}");
                }
                else
                {
                    Console.WriteLine($"Data from device: {device.station_name} is more than 20 minutes old.");
                }
            Console.ReadKey();
        }

        /// <summary>
        ///     Query the Netatmo API for temperature data.
        /// </summary>
        /// <param name="netatmoaccess_token">Netatmo token</param>
        /// <returns></returns>
        private static async Task<List<Device>> GetTempAsync(string netatmoaccess_token)
        {
            var response = "";
            var client = new HttpClient();
            var uri = new UriBuilder("https://api.netatmo.com/api/getstationsdata");
            uri.Query = $"access_token={netatmoaccess_token}";
            try
            {
                response = await client.GetStringAsync(uri.Uri);
                Log.Debug($"Response from Netatmo API: {response}");
            }
            catch (Exception e)
            {
                if (e.InnerException.Message.Contains("Forbidden"))
                {
                    Console.WriteLine("You are using invalid token");
                }
                else
                {
                    Console.WriteLine("Unexpected exception while sending request to Netatmo API");
                    Console.WriteLine(e.InnerException.Message);
                }
                Environment.Exit(1);
            }

            var jsonObject = JsonConvert.DeserializeObject<JObject>(response);
            var dataResult = jsonObject["body"].ToString();
            var cityData = JsonConvert.DeserializeObject<JObject>(dataResult);
            var devicesResult = cityData["devices"].ToString();
            var nDevice = JsonConvert.DeserializeObject<List<Device>>(devicesResult);
            return nDevice;
        }
    }
}