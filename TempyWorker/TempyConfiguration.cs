using System.IO;
using Microsoft.Extensions.Configuration;

namespace TempyWorker
{
    public class TempyConfiguration
    {
        public IConfigurationRoot ConfigurationRoot { get; set; }
        public string ConfigDirectory { get; set; }
        
        public TempyConfiguration(string _configDirectory = null)
        {
            if (_configDirectory == null)
            {
                ConfigDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                ConfigDirectory = _configDirectory;
            }
            
            ConfigurationRoot = GetConfigurationRoot();

        }
        public IConfigurationRoot GetConfigurationRoot()
        {
            // set up configuration
            IConfigurationRoot configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(ConfigDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            configuration = builder.Build();
            return configuration;
        }

    }
}