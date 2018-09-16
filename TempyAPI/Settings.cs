using System.IO;
using Newtonsoft.Json;

namespace TempyAPI
{
    public class SettingsStore
    {
        [JsonProperty("DocumentDBEndpointUri")]
        public string DocumentDBEndpointUri { get; set; }

        [JsonProperty("DocumentDBPrimaryKey")]
        public string DocumentDBPrimaryKey { get; set; }

        [JsonProperty("DocumentDBDatabaseId")]
        public string DocumentDBDatabaseId { get; set; }

        [JsonProperty("DocumentDBCollectionId")]
        public string DocumentDBCollectionId { get; set; }
    }

    public class Settings
    {
        //TODO: use better approach at handling settings, it should not be done here

        //TODO: build settings file path based on environment and test if file exists
        private readonly string settingsFIle = "/Users/fx/.tempy.settings";

        public Settings()
        {
            //TODO: test if file exists
            var rawSettings = File.ReadAllText(settingsFIle);


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