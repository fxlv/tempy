using System;
using NetatmoLib;
using Xunit;
using NetatmoLib;
using TempyWorker;
using System.IO;

namespace XUnitTestNetatmoLib
{
    public class NetatmoAuthTests
    {
        /// <summary>
        /// The auth token cannot be null. 
        /// </summary>
        [Fact]
        public void NetatmoAuthReturnsNonNullAuthToken()
        {
            // todo: this is really not the best place for this test, still it does provides value
            // it is possible for the authsettings.bin to contain a null token and this test will detect such an issue
            var cwd = Directory.GetCurrentDirectory();
            //var tempyWorkerBasePath = Path.GetFullPath(Path.Combine(cwd, @""));
            string configFileDirectory  = Path.GetFullPath(Path.Combine(cwd, @"../../../../TempyWorker"));
            
            var w = new Worker();
            var tempyConfiguration = new TempyConfiguration(configFileDirectory);
            var configuration = tempyConfiguration.GetConfigurationRoot();
            var netatmoCreds = tempyConfiguration.GetNetatmoApiAuthCredentials();
            var auth = new NetatmoAuth(netatmoCreds);
            // assert that the auth token is not null
            Assert.True(auth.GetToken() != null);

        }
    }
}