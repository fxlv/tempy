using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using NetatmoLib;

namespace TempyWorker
{
    public class TempyConfiguration
    {
        public IConfigurationRoot ConfigurationRoot { get; set; }
        public string ConfigDirectory { get; set; }
        public string ConfigurationFile { get; set; }

        public TempyConfiguration(string _configDirectory = null)
        {
            if (_configDirectory == null)
            {
                // unless configuration directory was provided to the constructor,
                // User the current working directory
                // support for overriding it exists mosty for use during testing
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

            if (File.Exists(primaryConfigFileName) && IsValidJsonFile(primaryConfigFileName))
            {
                ConfigurationFile = primaryConfigFileName;

            }
            else if (File.Exists(secondaryConfigFileName) && IsValidJsonFile(secondaryConfigFileName))
            {
                ConfigurationFile = secondaryConfigFileName;
            }
            else
            {
                throw new FileNotFoundException();
            }

            ConfigurationRoot = GetConfigurationRoot();

        }

        public bool IsValidJsonFile(string fileName)
        {
            string content = System.IO.File.ReadAllText(fileName);
            // trim whitespace from both ends
            content = content.Trim();
            if (content.StartsWith("{"))
            {
                if (content.EndsWith("}"))
                {
                    return true;
                }
            }

            return false;

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