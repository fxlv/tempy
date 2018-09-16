using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Serilog;

namespace NetatmoLib
{
    /// <summary>
    ///     Authentication class takes care of logging in to Netatmo
    ///     acquiring the Oauth token and saving it for re-use.
    /// </summary>
    public class NetatmoAuth
    {
        // TODO: move authsettings file name to a separate settings file
        private readonly string authSettingsFileName = "authsettings.bin";

        private OauthResponseObject oauthObject;

        /// <summary>
        ///     Constructor takes care of the business logic of
        ///     loading the saved authentication state and doing a re-auth if necessary.
        /// </summary>
        public NetatmoAuth()
        {
            oauthObject = new OauthResponseObject();
            LoadAuth();
            if (oauthObject.isValid)
            {
                Log.Debug("Auth settings loaded from file");
            }
            else
            {
                Log.Debug("Loaded auth settings were not valid. Will re-authenticate.");
                DoAuth();
                SaveAuth();
            }

            access_token = oauthObject.access_token;
        }

        // TODO: move this to a separate Configuration class
        public static IConfiguration Configuration { get; set; }

        private string access_token { get; }

        /// <summary>
        ///     Returns Netatmo auth token as string.
        /// </summary>
        public string GetToken()
        {
            return access_token;
        }

        /// <summary>
        ///     Save authentication state to a file locally for later re-use.
        /// </summary>
        private void SaveAuth()
        {
            oauthObject.timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            var toWrite = JsonConvert.SerializeObject(oauthObject);
            TextWriter writer = new StreamWriter("authsettings.bin", false);
            writer.Write(toWrite);
            writer.Close();
        }

        /// <summary>
        ///     Load authentication state from file.
        /// </summary>
        public void LoadAuth()
        {
            if (File.Exists(authSettingsFileName))
            {
                var timestampNow = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

                TextReader reader = null;
                try
                {
                    reader = new StreamReader(authSettingsFileName);
                    var contents = reader.ReadToEnd();
                    oauthObject = JsonConvert.DeserializeObject<OauthResponseObject>(contents);
                    Log.Debug($"Timestamp now: {timestampNow}, timestam from file: {oauthObject.timestamp}");
                    var timestampDelta = timestampNow - oauthObject.timestamp;
                    if (timestampDelta <= 10700)
                    {
                        Log.Debug($"We seem to have a valid auth token. Delta is {timestampDelta} seconds");
                        oauthObject.isValid = true;
                    }
                    else
                    {
                        oauthObject.isValid = false;
                    }
                }
                catch (FileNotFoundException)
                {
                    Log.Warning("Auth file not found");
                }
                catch (Exception e)
                {
                    Log.Warning("Exception while Loading auth file:");
                    Log.Warning(e.ToString());
                }
                finally
                {
                    if (reader != null) reader.Close();
                }
            }
            else
            {
                Log.Warning($"Auth settings file {authSettingsFileName} does not exist.");
            }
        }

        /// <summary>
        ///     Do oAuth authentication using locally stored credentials
        ///     against Netatmo oAuth API
        /// </summary>
        private void DoAuth()
        {
            var settingsFile = "appsettings.json";
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(settingsFile);

                Configuration = builder.Build();
            }
            catch (FormatException)
            {
                Log.Error($"Could not read '{settingsFile}'. Exiting.");
                Environment.Exit(1);
            }
            catch (FileNotFoundException)
            {
                Log.Error($"Settings file '{settingsFile}' not found. Exiting.");
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Log.Error($"Unexpected exception {e}");
                Environment.Exit(1);
            }

            var client = new RestClient("https://api.netatmo.com");
            client.Authenticator =
                new HttpBasicAuthenticator(Configuration["client_id"], Configuration["client_secret"]);
            var request = new RestRequest("oauth2/token", Method.POST);
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", Configuration["username"]);
            request.AddParameter("password", Configuration["password"]);

            Log.Debug("Making the request to Netatmo API");
            var response = client.Execute(request);
            var content = response.Content;
            Log.Debug(content);
            oauthObject = JsonConvert.DeserializeObject<OauthResponseObject>(content);
        }
    }
}