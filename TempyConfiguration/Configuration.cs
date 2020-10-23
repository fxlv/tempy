using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NetatmoLib;
using TempyLib;

namespace TempyConfiguration
{
    public class Configuration
    {
        public IConfigurationRoot ConfigurationRoot { get; set; }
        public string ConfigDirectory { get; set; }
        public string ConfigurationFile { get; set; }

        public Configuration(string _configDirectory = null)
        {
            if (_configDirectory == null)
            {
                // unless configuration directory was provided to the constructor,
                // User the current working directory
                // support for overriding it exists mostly for use during testing
                ConfigDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                ConfigDirectory = _configDirectory;
            }

            // find the configuration file
            // in case the primary configuration file is missing, fall back to using appsettings.json.default
            // if neither are present, throw an exception

            string primaryConfigFileName = Path.Combine(ConfigDirectory, "appsettings.json");
            string secondaryConfigFileName = Path.Combine(ConfigDirectory, "appsettings.json.default");
            string kubernetesConfigFileName = "/config/appsettings.json";


            if (File.Exists(kubernetesConfigFileName) && Validators.IsValidJsonFile(kubernetesConfigFileName))
            {
                // running in kubernetes and settings file has been provided via a config map
                ConfigurationFile = kubernetesConfigFileName;
            }
            else if (File.Exists(primaryConfigFileName) && Validators.IsValidJsonFile(primaryConfigFileName))
            {
                ConfigurationFile = primaryConfigFileName;

            }
            else if (File.Exists(secondaryConfigFileName) && Validators.IsValidJsonFile(secondaryConfigFileName))
            {
                ConfigurationFile = secondaryConfigFileName;
            }
            else
            {
                throw new FileNotFoundException();
            }

            ConfigurationRoot = GetConfigurationRoot();

        }
        

        public IConfigurationRoot GetConfigurationRoot()
        {
            // set up configuration
            IConfigurationRoot configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(ConfigDirectory)
                .AddJsonFile(ConfigurationFile)
                .AddEnvironmentVariables();
            configuration = builder.Build();
            return configuration;
        }

        public NetatmoApiAuthCredentials GetNetatmoApiAuthCredentials()
        {
            // instantiate netatmoCreds object that will keep our credentials
            var netatmoCreds = new NetatmoApiAuthCredentials();
            // populate the credentials from the configuration
            ConfigurationRoot.GetSection("netatmo_api_auth").Bind(netatmoCreds);
            return netatmoCreds;
        }

        public LoggingConfig GetLoggingConfig()
        {
            var loggingConfig = new LoggingConfig();
            ConfigurationRoot.GetSection("logging").Bind(loggingConfig);
            if (loggingConfig.IsValid)
            {
                return loggingConfig;
            }
            else
            {
                throw new Exception("Invalid or incomplete logging config provided!");
            }
        }

    }

    public class LoggingConfig
    {
        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }

        public string LogFilePath
        {
            get
            {
                if (IsValid)
                {
                    return Path.Combine(LogDirectory, LogFileName);
                }

                return null;
            }
        }

        public bool IsValid
        {
            get
            {
                if (LogDirectory == null || LogFileName == null)
                {
                    return false;
                }

                return true;
            }
        }
    }
}