using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading.Tasks;
using Serilog;

namespace netatmo {

    class Program {

        static async Task Main (string[] args) {

            // Set up logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information).WriteTo.File("netatmo.log", rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug("Logging started");
            NetatmoAuth netAuth = new NetatmoAuth ();
            await DisplayTemp(netAuth);
            Log.CloseAndFlush();
        }
        
        /// <summary>
        /// Determine if the data is fresh by looking at timestamps
        /// Fresh here means, not older than 20 minutes.
        /// </summary>
        static bool isDataFresh(int unixTimestamp){
            int maxAllowedDiff = 1200; // 20 minutes
            var timeStamp = GetDateTimeOffset(unixTimestamp);
            var diff = GetTimeDelta(timeStamp);
            if (diff.TotalSeconds < maxAllowedDiff){
                return true;
            }
            return false;
        }

        /// <summary>
        /// Convert unixTimestamp (integer) to DateTimeOffset
        /// </summary>
        static DateTimeOffset GetDateTimeOffset(int unixTimestamp){
            var timeStamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return timeStamp;
        }

        /// <summary>
        /// Calculate the difference between "now" and the provided timestamp
        /// </summary>
        static TimeSpan GetTimeDelta(DateTimeOffset timeStamp){
             var now = DateTime.Now;
             var diff = now - timeStamp;
             return diff;
        }
        /// <summary>
        /// Return a human readable string, such as "5 minutes" or "1 minute" etc.
        /// based on the unix timestamp
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns>String representation of last update time</returns>
        static string GetLastUpdateString(int unixTimestamp){
            var timeStamp = GetDateTimeOffset(unixTimestamp);
            var delta = GetTimeDelta(timeStamp);
            string humanReadableString = "unknown";
            double deltaMinutes = Math.Round(delta.TotalMinutes);
            if (delta.TotalSeconds < 60){
                humanReadableString = "less than a minute";
            } else if (deltaMinutes == 1){
                humanReadableString = "a minute";
            } else if (deltaMinutes > 1){
                humanReadableString = $"{deltaMinutes} minutes";
            }
            return humanReadableString;
        }
        /// <summary>
        /// Display temperature data on the CLI.
        /// </summary>
        static async Task DisplayTemp (NetatmoAuth netAuth) {
            List<Device> netatmoDevices = await GetTempAsync (netAuth.GetToken ());

            Console.WriteLine(new String('-',60));
            foreach (Device device in netatmoDevices) {
                if(isDataFresh(device.last_status_store)){
                    var lastUpdateString = GetLastUpdateString(device.last_status_store);
                    Console.WriteLine ($"{device.station_name} (updated {lastUpdateString} ago)");
                    Console.WriteLine($" Sensor: {device.module_name }, temperature: {device.dashboard_data.Temperature}");
                    foreach (Module module in device.modules) {
                        Console.WriteLine ($" Sensor: {module.module_name}, Temperature: {module.dashboard_data.Temperature}");
                    }
                } else {
                    Console.WriteLine ($"Data from device: {device.station_name} is more than 20 minutes old.");
                }
            }
        }
        /// <summary>
        /// Query the Netatmo API for temperature data.
        /// </summary>
        /// <param name="netatmoaccess_token">Netatmo token</param>
        /// <returns></returns>
        static async Task<List<Device>> GetTempAsync (string netatmoaccess_token) {
            string response = "";
            HttpClient client = new HttpClient ();
            UriBuilder uri = new UriBuilder ("https://api.netatmo.com/api/getstationsdata");
            uri.Query = $"access_token={netatmoaccess_token}";
            try {
                response = await client.GetStringAsync (uri.Uri);
                Log.Debug($"Response from Netatmo API: {response}");
            } catch (Exception e) {
                if (e.InnerException.Message.Contains ("Forbidden")) {
                    Console.WriteLine ("You are using invalid token");
                } else {
                    Console.WriteLine ("Unexpected exception while sending request to Netatmo API");
                    Console.WriteLine (e.InnerException.Message);
                }
                Environment.Exit (1);
            }

            var jsonObject = JsonConvert.DeserializeObject<JObject> (response);
            var dataResult = jsonObject["body"].ToString ();
            var cityData = JsonConvert.DeserializeObject<JObject> (dataResult);
            var devicesResult = cityData["devices"].ToString ();
            List<Device> nDevice = JsonConvert.DeserializeObject<List<Device>> (devicesResult);
            return nDevice;
        }
    }
}