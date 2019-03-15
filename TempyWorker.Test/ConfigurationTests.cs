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
            var cwd = Directory.GetCurrentDirectory();
            var tempyWorkerBasePath = Path.GetFullPath(Path.Combine(cwd, @"../../../../TempyWorker"));
            TempyConfiguration = new TempyConfiguration(tempyWorkerBasePath);
            Configuration = TempyConfiguration.GetConfigurationRoot();
        }

        public static IConfigurationRoot Configuration { get; set; }
        public Worker Worker;
        public TempyConfiguration TempyConfiguration;

        [Fact]
        public void GetLoggingConfig()
        {
            var loggingConfig = TempyLogger.GetLoggingConfig(Configuration);
            Assert.IsType<string>(loggingConfig.LogDirectory);
            Assert.IsType<string>(loggingConfig.LogFileName);


        }
        [Fact]
        public void GetNetatmoApiAuthCredentials()
        {
           

            var netatmoCreds = Worker.GetNetatmoApiAuthCredentials(Configuration);

            Assert.IsType<string>(netatmoCreds.Username);
            Assert.IsType<string>(netatmoCreds.Password);
            Assert.IsType<string>(netatmoCreds.ClientId);
            Assert.IsType<string>(netatmoCreds.ClientSecret);
        }

        [Fact]
        public void ThrowExceptionWhenConfigurationFileIsMissing()
        {
            // test scenario when configuration file is missing
            Assert.Throws<FileNotFoundException>(() => new TempyConfiguration(Directory.GetCurrentDirectory()));
        }
    }
}