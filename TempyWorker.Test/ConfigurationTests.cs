using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;
using TempyConfiguration;
using TempyLogger;

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
            TempyConfiguration = new Configuration(configFileDirectory);
            Configuration = TempyConfiguration.GetConfigurationRoot();
        }

        public static IConfigurationRoot Configuration { get; set; }
        public Worker Worker;
        public Configuration TempyConfiguration;
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

            var netatmoCreds = TempyConfiguration.GetNetatmoApiAuthCredentials();

            Assert.IsType<string>(netatmoCreds.Username);
            Assert.IsType<string>(netatmoCreds.Password);
            Assert.IsType<string>(netatmoCreds.ClientId);
            Assert.IsType<string>(netatmoCreds.ClientSecret);
        }
        
        
        /// <summary>
        /// If neither appsettings.json or appsettings.json.default can be found, we throw in the towel.
        /// </summary>
        [Fact]
        public void ThrowExceptionWhenConfigurationFilesAreMissing()
        {
           string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "both_missing"));        
           Assert.Throws<FileNotFoundException>(() => new Configuration(configFileDirectory));
        }

        /// <summary>
        /// Validate that in case when logging config is invalid or missing, Tempy throws and exception
        /// with a clarifying message
        /// </summary>
        [Fact]
        public void MissingLoggingConfigThrowsException()
        {
            
            string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "logging_config_missing")); 
            Configuration tConfiguration = new Configuration(configFileDirectory);

            Assert.Throws<System.Exception>(() => Logger.Initilize(tConfiguration));
        }

        /// <summary>
        /// In case the logging config is invalid or malformed we expect an exception to be thrown
        /// because the property LogFilePath wont be constructed
        /// </summary>
        [Fact]
        public void InvalidLoggingConfigSectionThrowsException()
        {
            string configFileDirectory = Path.GetFullPath(Path.Combine(testFilesDir, "logging_config_incomplete")); 
            Configuration tConfiguration = new Configuration(configFileDirectory);
            // so far so good, the config was constructed, but it is missing the logging part
            // we now try to initialize logging, this will fail, because logging initializer expects
            // LogFilePath, which cannot be constructed now due to the missing logging config
            Assert.Throws<System.Exception>( () => Logger.Initilize(tConfiguration));

        }
    }
}