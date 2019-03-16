using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;
using Serilog.Configuration;

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
                
            } else if (File.Exists(secondaryConfigFileName) && IsValidJsonFile(secondaryConfigFileName))
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

        /// <summary>
        /// Validate theat the values in logging config are not null.
        /// </summary>
        /// <param name="loggingConfig"></param>
        /// <returns></returns>
        public bool IsValidLoggingConfig(LoggingConfig loggingConfig)
        {

            if (loggingConfig.LogDirectory == null || loggingConfig.LogFileName == null)
            {
                return false;
            }

            return true;
        }
        public LoggingConfig GetLoggingConfig()
        {
           var loggingConfig = new LoggingConfig();
           ConfigurationRoot.GetSection("logging").Bind(loggingConfig);
            if (IsValidLoggingConfig(loggingConfig))
            {
                return loggingConfig;
            }
            else
            {
                throw new Exception("Invalid logging config provided!");
            }
        }

    }
    
    public class LoggingConfig
    {
        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }
    }
}