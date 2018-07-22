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

namespace netatmo {

    class Program {
        public static IConfiguration Configuration { get; set; }

        static void Main (string[] args) {
            var oauthResponse = LoadAuth ();
            if (oauthResponse.isValid) {
                Console.WriteLine ("Auth settings loaded from file");
            } else {
                oauthResponse = DoAuth ();
                SaveAuth (oauthResponse);
            }

            GetTemp (oauthResponse.access_token);
        }

        static void SaveAuth (OauthResponseObject oauthObject) {
            oauthObject.timestamp = new DateTimeOffset (DateTime.UtcNow).ToUnixTimeSeconds ();

            var toWrite = JsonConvert.SerializeObject (oauthObject);
            TextWriter writer = new StreamWriter ("authsettings.bin", false);
            writer.Write (toWrite);
            writer.Close ();
        }

        static OauthResponseObject LoadAuth () {
            OauthResponseObject oauthObject = new OauthResponseObject ();
            long timestampNow = new DateTimeOffset (DateTime.UtcNow).ToUnixTimeSeconds ();

            TextReader reader = null;
            try {
                reader = new StreamReader ("authsettings.bin");
                var contents = reader.ReadToEnd ();
                oauthObject = JsonConvert.DeserializeObject<OauthResponseObject> (contents);
                Console.WriteLine ($"Timestamp now: {timestampNow}, timestam from file: {oauthObject.timestamp}");
                var timestampDelta = timestampNow - oauthObject.timestamp;
                if (timestampDelta <= 10700) {
                    Console.WriteLine ($"We seem to have a valid auth token. Delta is {timestampDelta} seconds");
                    oauthObject.isValid = true;
                } else {
                    oauthObject.isValid = false;
                }
            } catch (System.IO.FileNotFoundException e) {
                Console.WriteLine ("Auth file not found");
            } catch (Exception e) {
                Console.WriteLine ("Exception while Loading auth file:");
                Console.WriteLine (e);

            } finally {
                if (reader != null) {
                    reader.Close ();
                }
            }
            return oauthObject;
        }
        static OauthResponseObject DoAuth () {
            string settingsFile = "appsettings.json";
            try {
                var builder = new ConfigurationBuilder ()
                    .SetBasePath (Directory.GetCurrentDirectory ())
                    .AddJsonFile (settingsFile);

                Configuration = builder.Build ();
            } catch (System.FormatException e) {
                Console.WriteLine ($"Could not read '{settingsFile}'");
                Environment.Exit (1);
            } catch (System.IO.FileNotFoundException e) {
                Console.WriteLine ($"Settings file '{settingsFile}' not found");
                Environment.Exit (0);
            } catch (Exception e) {
                Console.WriteLine ($"Unexpected exception {e}");
                Environment.Exit (1);
            }

            var client = new RestClient ("https://api.netatmo.com");
            client.Authenticator = new HttpBasicAuthenticator (Configuration["client_id"], Configuration["client_secret"]);
            var request = new RestRequest ("oauth2/token", Method.POST);
            request.AddParameter ("grant_type", "password");
            request.AddParameter ("username", Configuration["username"]);
            request.AddParameter ("password", Configuration["password"]);

            Console.WriteLine ("Making the request to Netatmo API");
            IRestResponse response = client.Execute (request);
            var content = response.Content;
            Console.WriteLine (content);
            OauthResponseObject oauthResponse = JsonConvert.DeserializeObject<OauthResponseObject> (content);
            Console.WriteLine (content);
            return oauthResponse;

        }

        static async void GetTemp (string netatmoaccess_token) {
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
            foreach (Device device in nDevice) {
                Console.WriteLine ($"{device.station_name}, Temperature: {device.dashboard_data.Temperature}");
                foreach (Module module in device.modules) {
                    Console.WriteLine ($" Module: {module.module_name}, Temperature: {module.dashboard_data.Temperature}");
                }
            }
        }
    }
}