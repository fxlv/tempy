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

namespace netatmo {

    class Program {

        static async Task Main (string[] args) {

            NetatmoAuth netAuth = new NetatmoAuth ();
            await DisplayTemp(netAuth);
        }
        /// <summary>
        /// Display temperature data on the CLI.
        /// </summary>
        static async Task DisplayTemp (NetatmoAuth netAuth) {
            List<Device> netatmoDevices = await GetTempAsync (netAuth.GetToken ());

            foreach (Device device in netatmoDevices) {
                Console.WriteLine ($"{device.station_name}, Temperature: {device.dashboard_data.Temperature}");
                foreach (Module module in device.modules) {
                    Console.WriteLine ($" Module: {module.module_name}, Temperature: {module.dashboard_data.Temperature}");
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
                response = client.GetStringAsync (uri.Uri).Result;
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