using System;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NetatmoLib;
using Xunit;
using System.IO;
using TempyWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Binder;

namespace TempyWorker.Test
{
    public class ConfigurationTests
    {
        public static IConfigurationRoot Configuration { get; set; }
        public TempyWorker.Worker Worker;

        public ConfigurationTests()
        {
            Worker = new Worker();
            var cwd = Directory.GetCurrentDirectory();
            var tempyWorkerBasePath = Path.GetFullPath(Path.Combine(cwd, @"../../../../TempyWorker"));
            Configuration = Worker.GetConfigurationRoot(tempyWorkerBasePath);
        }

        [Fact]
        public void ThrowExceptionWhenConfigurationFileIsMissing()
        {
            // test scenario when configuration file is missing
        
            Assert.Throws<System.IO.FileNotFoundException>(() => Worker.GetConfigurationRoot(Directory.GetCurrentDirectory()) );
        }
        
        [Fact]
        public void GetNetatmoApiAuthCredentials()
        {
            
            // arrange
            
            // act

            var netatmoCreds = Worker.GetNetatmoApiAuthCredentials(Configuration);
            
            // assert
            Assert.IsType<string>(netatmoCreds.Username);
            Assert.IsType<string>(netatmoCreds.Password);
            Assert.IsType<string>(netatmoCreds.ClientId);
            Assert.IsType<string>(netatmoCreds.ClientSecret);

        }
    }
}