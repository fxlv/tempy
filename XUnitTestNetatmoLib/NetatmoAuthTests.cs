using System;
using System.Collections.Generic;
using NetatmoLib;
using Xunit;
using TempyWorker;
using System.IO;
using TempyConfiguration;
using TempyLib;

namespace XUnitTestNetatmoLib
{
    public class NetatmoAuthTests
    {
        /// <summary>
        /// The auth token cannot be null. 
        /// </summary>
        [Fact (Skip = "needs authentication token to work")]
        public void NetatmoAuthReturnsNonNullAuthToken()
        {
            // todo: this is really not the best place for this test, still it does provides value
            // it is possible for the authsettings.bin to contain a null token and this test will detect such an issue
            var cwd = Directory.GetCurrentDirectory();
            //var tempyWorkerBasePath = Path.GetFullPath(Path.Combine(cwd, @""));
            string configFileDirectory  = Path.GetFullPath(Path.Combine(cwd, @"../../../../TempyWorker"));
            
            var w = new Worker();
            var tempyConfiguration = new Configuration(configFileDirectory);
            var configuration = tempyConfiguration.GetConfigurationRoot();
            var netatmoCreds = tempyConfiguration.GetNetatmoApiAuthCredentials();
            if (netatmoCreds.Password == "password")
            {
                // default password, there is no point to even try to authenticate
                return;
            } 
            var auth = new NetatmoAuth(netatmoCreds);
            // assert that the auth token is not null
            Assert.True(auth.GetToken() != null);

        }
    }

    public class NetatmoQueriesTests
    {
        
        [Fact]
        public void ExtractingBodyFromJsonStringReturnsValidResult()
        {          
            // read in an example json response from a file
            var cwd = Directory.GetCurrentDirectory();
            var path = Path.Combine(cwd, @"TestFiles/netatmo_response_body.json");
            var content = Validators.ReadFile(path);
            var expectedString = "{\n  \"devices\": []\n}";
            // act   
            var actual = NetatmoQueries.GetJsonBody(content);
            // assert
            Assert.Equal(expectedString, actual);
        
        }
        
        [Fact]
        public void ParsingValidDoubleNetatmoResponseStringReturnsListOfDevices()
        {          
            // read in an example json response from a file
            var cwd = Directory.GetCurrentDirectory();
            var path = Path.Combine(cwd, @"TestFiles/netatmo_response_double.json");
            var content = Validators.ReadFile(path);
            // act   
            var actual = NetatmoQueries.ParseResponse(content);
            // assert
            Assert.IsType<List<Device>>(actual);
            Assert.True(actual.Count == 2);
        }
        
        [Fact]
        public void ParsingDoubleIncompleteNetatmoResponseIsHandledGracefully()
        {          
            // read in an example json response from a file
            var cwd = Directory.GetCurrentDirectory();
            var path = Path.Combine(cwd, @"TestFiles/netatmo_response_double_incomplete.json");
            var content = Validators.ReadFile(path);
            // act   
            var actual = NetatmoQueries.ParseResponse(content);
            // assert
            Assert.IsType<List<Device>>(actual);
            Assert.True(actual.Count == 2);
        }
        
        [Fact]
        public void ParsingValidSingleNetatmoResponseStringReturnsListOfDevices()
        {          
            // read in an example json response from a file
            var cwd = Directory.GetCurrentDirectory();
            var path = Path.Combine(cwd, @"TestFiles/netatmo_response_single.json");
            var content = Validators.ReadFile(path);
            // act   
            var actual = NetatmoQueries.ParseResponse(content);
            // assert
            Assert.IsType<List<Device>>(actual);
            Assert.True(actual.Count == 1);
        }
    }
}