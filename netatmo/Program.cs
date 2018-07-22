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

        public class DefaultAlarm {
            public int db_alarm_number { get; set; }
        }

        #region Netatmo Oauth response 
        public class OauthResponseObject {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public List<string> scope { get; set; }
            public int expires_in { get; set; }
            public int expire_in { get; set; }
            public bool isValid { get; set; } = false;
            public long timestamp { get; set; }
        }
        #endregion
        #region Netatmo object definition
        public class AlarmConfig {
            public List<DefaultAlarm> default_alarm { get; set; }
            public List<object> personnalized { get; set; }
        }

        public class DashboardData {
            public double AbsolutePressure { get; set; }
            public int CO2 { get; set; }
            public int Humidity { get; set; }
            public int Noise { get; set; }
            public double Pressure { get; set; }
            public double Temperature { get; set; }
            public int date_max_temp { get; set; }
            public int date_min_temp { get; set; }
            public double max_temp { get; set; }
            public double min_temp { get; set; }
            public string pressure_trend { get; set; }
            public string temp_trend { get; set; }
            public int time_utc { get; set; }
        }

        public class DashboardData2 {
            public int Humidity { get; set; }
            public double Temperature { get; set; }
            public int date_max_temp { get; set; }
            public int date_min_temp { get; set; }
            public double max_temp { get; set; }
            public double min_temp { get; set; }
            public string temp_trend { get; set; }
            public int time_utc { get; set; }
            public int? CO2 { get; set; }
        }

        public class Module {
            public string _id { get; set; }
            public int battery_percent { get; set; }
            public int battery_vp { get; set; }
            public DashboardData2 dashboard_data { get; set; }
            public List<string> data_type { get; set; }
            public int firmware { get; set; }
            public int last_message { get; set; }
            public int last_seen { get; set; }
            public int last_setup { get; set; }
            public string module_name { get; set; }
            public int rf_status { get; set; }
            public string type { get; set; }
        }

        public class Place {
            public string city { get; set; }
            public string country { get; set; }
            public List<double> location { get; set; }
            public string timezone { get; set; }
            public bool? improveLocProposed { get; set; }
        }

        public class Device {
            public string _id { get; set; }
            public string access_code { get; set; }
            public bool air_quality_available { get; set; }
            public AlarmConfig alarm_config { get; set; }
            public string cipher_id { get; set; }
            public bool co2_calibrating { get; set; }
            public DashboardData dashboard_data { get; set; }
            public List<string> data_type { get; set; }
            public int date_setup { get; set; }
            public int firmware { get; set; }
            public List<string> friend_users { get; set; }
            public bool invitation_disable { get; set; }
            public int last_setup { get; set; }
            public int last_status_store { get; set; }
            public int last_upgrade { get; set; }
            public string module_name { get; set; }
            public List<Module> modules { get; set; }
            public Place place { get; set; }
            public bool public_ext_data { get; set; }
            public string station_name { get; set; }
            public string type { get; set; }
            public List<string> user_owner { get; set; }
            public int wifi_status { get; set; }
        }

        public class Administrative {
            public int feel_like_algo { get; set; }
            public string lang { get; set; }
            public int pressureunit { get; set; }
            public string reg_locale { get; set; }
            public int unit { get; set; }
            public int windunit { get; set; }
        }

        public class User {
            public Administrative administrative { get; set; }
            public bool fb_chatbot_available { get; set; }
            public string mail { get; set; }
        }

        public class Body {
            public List<Device> devices { get; set; }
            public User user { get; set; }
        }

        #endregion

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