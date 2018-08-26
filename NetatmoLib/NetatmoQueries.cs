using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NetatmoLib
{
    public class NetatmoQueries
    {
        /// <summary>
        ///     Query the Netatmo API for temperature data.
        /// </summary>
        /// <param name="netatmoaccess_token">Netatmo token</param>
        /// <returns></returns>
        public static async Task<List<Device>> GetTempAsync(string netatmoaccess_token)
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
                    // TODO: No writing to Console from within a library
                    Console.WriteLine("You are using invalid token");
                }
                else
                {
                    // TODO: No writing to Console from within a library
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
