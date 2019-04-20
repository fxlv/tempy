using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NetatmoLib
{
    public static class NetatmoQueries
    {
        /// <summary>
        ///     Query the Netatmo API for temperature data.
        /// </summary>
        /// <param name="netatmoaccessToken">Netatmo token</param>
        /// <returns></returns>
        public static async Task<List<Device>> GetTempAsync(string netatmoaccessToken)
        {
            var response = "";
            var client = new HttpClient();
            var uri = new UriBuilder("https://api.netatmo.com/api/getstationsdata");
            uri.Query = $"access_token={netatmoaccessToken}";
            try
            {
                response = await client.GetStringAsync(uri.Uri);
                Log.Debug($"Response from Netatmo API: {response}");
            }
            catch (Exception e)
            {
                if (e.InnerException.Message.Contains("Forbidden"))
                {
                    // TODO: No writing to Console from within a library
                    Console.WriteLine("You are using invalid token");
                }
                else
                {
                    // TODO: No writing to Console from within a library
                    Console.WriteLine("Unexpected exception while sending request to Netatmo API");
                    Console.WriteLine(e.InnerException.Message);
                }
                // TODO: should not do this, rather catch exception in the CLI APP!
                Environment.Exit(1);
            }

            return ParseResponse(response);
        }

        /// <summary>
        /// Parse a JSON string and return its 'devices [ {} ]' part
        /// </summary>
        /// <param name="rawJson"></param>
        /// <returns></returns>
        public static string GetDevices(string rawJson)
        {
            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(rawJson);
            string devices = jsonObject["devices"].ToString();
            return devices;
        }
        
        /// <summary>
        /// Parse a JSON string and return its 'body {}' part
        /// </summary>
        /// <param name="rawJson"></param>
        /// <returns>string containing the body part</returns>
        public static string GetJsonBody(string rawJson)
        {
            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(rawJson);
            string result = jsonObject["body"].ToString();
            return result;
        }
        /// <summary>
        /// Parse response string from Netatmo API
        /// </summary>
        /// <param name="response"></param>
        /// <returns>List of Devices</returns>
        public static List<Device> ParseResponse(string response)
        {
            // TODO: handle cases where invalid or incomplete result has been returned by the netatmo api
            // ( for example: missing measurement from external sensor )
            string jsonBody = GetJsonBody(response);
            string jsonDevices = GetDevices(jsonBody);
            // deserialize the json devices list into a List<Device>>
            var devices = JsonConvert.DeserializeObject<List<Device>>(jsonDevices);
            return devices;
        }
    }
}