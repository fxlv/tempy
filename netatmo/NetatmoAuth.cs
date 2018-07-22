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

    /// <summary>
    /// Authentication class takes care of logging in to Netatmo
    /// acquiring the Oauth token and saving it for re-use.
    /// </summary>
    class NetatmoAuth {

        // TODO: move this to a separate Configuration class
        public static IConfiguration Configuration { get; set; }
        private string authSettingsFileName = "authsettings.bin";
        private string access_token { get; set; }

        private OauthResponseObject oauthObject;
        /// <summary>
        /// Constructor takes care of the business logic of
        /// loading the saved authentication state and doing a re-auth if necessary.
        /// </summary>
        public NetatmoAuth () {
            oauthObject = new OauthResponseObject ();
            LoadAuth ();
            if (oauthObject.isValid) {
                Console.WriteLine ("Auth settings loaded from file");
            } else {
                Console.WriteLine ("Loaded auth settings were not valid. Will re-authenticate.");
                DoAuth ();
                SaveAuth ();
            }
            access_token = oauthObject.access_token;

        }
        /// <summary>
        /// Returns Netatmo auth token as string.
        /// </summary>
        public string GetToken () {
            return access_token;
        }
        /// <summary>
        /// Save authentication state to a file locally for later re-use.
        /// </summary>
        private void SaveAuth () {
            oauthObject.timestamp = new DateTimeOffset (DateTime.UtcNow).ToUnixTimeSeconds ();

            var toWrite = JsonConvert.SerializeObject (oauthObject);
            TextWriter writer = new StreamWriter ("authsettings.bin", false);
            writer.Write (toWrite);
            writer.Close ();
        }
        /// <summary>
        /// Load authentication state from file.
        /// </summary>
        public void LoadAuth () {

            if (File.Exists (authSettingsFileName)) {
                long timestampNow = new DateTimeOffset (DateTime.UtcNow).ToUnixTimeSeconds ();

                TextReader reader = null;
                try {
                    reader = new StreamReader (authSettingsFileName);
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
            } else {
                Console.WriteLine($"Auth settings file {authSettingsFileName} does not exist.");
            }
        }
        /// <summary>
        /// Do oAuth authentication using locally stored credentials
        /// against Netatmo oAuth API
        /// </summary>
        private void DoAuth () {
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
            oauthObject = JsonConvert.DeserializeObject<OauthResponseObject> (content);
        }
    }
}