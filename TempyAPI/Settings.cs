using System;
using System.IO;
using Newtonsoft.Json;

namespace TempyAPI
{
    public class SettingsStore
    {
        [JsonProperty("DocumentDBEndpointUri")]
        public string DocumentDBEndpointUri { get; set; }

        [JsonProperty("DocumentDBPrimaryKey")] public string DocumentDBPrimaryKey { get; set; }

        [JsonProperty("DocumentDBDatabaseId")] public string DocumentDBDatabaseId { get; set; }

        [JsonProperty("DocumentDBCollectionId")]
        public string DocumentDBCollectionId { get; set; }
    }

    public class Settings
    {
        //TODO: use better approach at handling settings, it should not be done here


        
        public Settings(string contentRoot)
        {
            string settingsFileName = ".tempy.settings";
            string settingsFile = Path.Combine(contentRoot, settingsFileName);
            if(!File.Exists(settingsFile)){
                throw new System.IO.FileNotFoundException("Cannot find settings file");
            }
            var rawSettings = File.ReadAllText(settingsFile);


            var parsedSettings = JsonConvert.DeserializeObject<SettingsStore>(rawSettings);
            DocumentDBCollectionId = parsedSettings.DocumentDBCollectionId;
            DocumentDBDatabaseId = parsedSettings.DocumentDBDatabaseId;
            DocumentDBPrimaryKey = parsedSettings.DocumentDBPrimaryKey;
            DocumentDBEndpointUri = parsedSettings.DocumentDBEndpointUri;
        }

        public string DocumentDBEndpointUri { get; }
        public string DocumentDBPrimaryKey { get; }
        public string DocumentDBDatabaseId { get; }
        public string DocumentDBCollectionId { get; }
    }
}