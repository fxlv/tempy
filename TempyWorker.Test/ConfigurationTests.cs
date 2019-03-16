using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace TempyWorker.Test
{
    public class ConfigurationTests
    {
        public ConfigurationTests()
        {
            Worker = new Worker();
            cwd = Directory.GetCurrentDirectory();
            //var tempyWorkerBasePath = Path.GetFullPath(Path.Combine(cwd, @""));
            testFilesDir = Path.GetFullPath(Path.Combine(cwd, @"../../../../TempyWorker.Test/TestFiles/"));
            string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "both_exist"));
            TempyConfiguration = new TempyConfiguration(configFileDirectory);
            Configuration = TempyConfiguration.GetConfigurationRoot();
        }

        public static IConfigurationRoot Configuration { get; set; }
        public Worker Worker;
        public TempyConfiguration TempyConfiguration;
        public string cwd { get; set;  }
        public string testFilesDir { get; set;  }


        [Fact]
        public void GetLoggingConfig()
        {
            var loggingConfig = TempyConfiguration.GetLoggingConfig();
            Assert.IsType<string>(loggingConfig.LogDirectory);
            Assert.IsType<string>(loggingConfig.LogFileName);
        }

        /// <summary>
        /// Validate that configuration contains settings that are needed by netatmo auth
        /// </summary>
        [Fact]
        public void GetNetatmoApiAuthCredentials()
        {
            var netatmoCreds = Worker.GetNetatmoApiAuthCredentials(Configuration);

            Assert.IsType<string>(netatmoCreds.Username);
            Assert.IsType<string>(netatmoCreds.Password);
            Assert.IsType<string>(netatmoCreds.ClientId);
            Assert.IsType<string>(netatmoCreds.ClientSecret);
        }

        /// <summary>
        /// Test scenario when the config file appsettings.json is missing.
        /// In such scenario Tempy should fall back to using appsettings.json.default
        /// </summary>
        [Fact]
        public void FallBackToDefaultConfigFile_when_config_missing()
        {
            string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "default_missing")); 
            TempyConfiguration = new TempyConfiguration(configFileDirectory);
            // get the currently used config file base name
            var configBaseName = Path.GetFileName(TempyConfiguration.ConfigurationFile); 
            // assert we have fallen back to using the default config file
            Assert.True(configBaseName == "appsettings.json.default"); 
                
        }
        
        /// <summary>
        /// Test a scenario when appsettings.json file is invalid. For example when it is encrypted or malformed.
        /// In such scenario Tempy should fall back to using appsettings.json.default
        /// </summary>
        [Fact]
        public void FallBackToDefaultConfigFile_when_config_corrupted()
        {
            string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "default_corrupted")); 
            TempyConfiguration = new TempyConfiguration(configFileDirectory);
            // get the currently used config file base name
            var configBaseName = Path.GetFileName(TempyConfiguration.ConfigurationFile); 
            // assert we have fallen back to using the default config file
            Assert.True(configBaseName == "appsettings.json.default"); 
        }
        
        /// <summary>
        /// If neither appsettings.json or appsettings.json.default can be found, we throw in the towel.
        /// </summary>
        [Fact]
        public void ThrowExceptionWhenConfigurationFilesAreMissing()
        {
           string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "both_missing"));        
           Assert.Throws<FileNotFoundException>(() => new TempyConfiguration(configFileDirectory));
        }
    }
}