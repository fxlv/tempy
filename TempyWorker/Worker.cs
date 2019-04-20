using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NetatmoLib;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using TempyAPI;
using TempyConfiguration;

namespace TempyWorker
{
    public class Worker
    {


        public class RunnerConfig
        {
            public string TempyApiTarget { get; set; }
            public NetatmoApiAuthCredentials NetatmoCreds { get; set;  }
            public int SleepSeconds { get; set; }
            public int FailSleep = 30;

            public RunnerConfig(string tempyApiTarget, NetatmoApiAuthCredentials netatmoCreds, int sleepSeconds = 300)
            {
                NetatmoCreds = netatmoCreds;
                TempyApiTarget = tempyApiTarget;
                SleepSeconds = sleepSeconds;
            }
        }
        
        public void Run()
        {
            var tempyConfiguration = new Configuration();
            var configuration = tempyConfiguration.GetConfigurationRoot();
            var netatmoCreds = tempyConfiguration.GetNetatmoApiAuthCredentials();

            var tempyApiTarget = Environment.GetEnvironmentVariable("TEMPY_API_TARGET");
            if (tempyApiTarget == null) tempyApiTarget = "localhost:5000";
            
            
            var runnerConfig = new RunnerConfig(tempyApiTarget,netatmoCreds);

            WorkerRunner(runnerConfig);            
        }

        public void WorkerRunner(RunnerConfig runnerConfig)
        {
            // run the actual worker runs
            // check their exit codes
            // provide sleep between runs and do backoff in case api status is not 200
            int failCounter = 0;
            
            while (true)
            {
                if (CheckApiStatus(runnerConfig))
                {
                    WorkerRun(runnerConfig);
                    failCounter = 0; // reset fail counter
                }
                else
                {
                    Log.Warning("Tempy API not available.");
                    failCounter += 1;
                }
                int sleepSeconds = runnerConfig.SleepSeconds + (failCounter * runnerConfig.FailSleep);
                Log.Debug($"Sleeping for {sleepSeconds} seconds");
                sleepSeconds = sleepSeconds * 1000;
                Thread.Sleep(sleepSeconds);
            }
        }
        public async void WorkerRun(RunnerConfig runnerConfig)
        {
            // do netatmo auth
            // initializes netatmolib
            // queries netatmo for data about all sensors
            // iterates over result
            // for each result:
            // send result to tempy API 
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Log.Debug("Doing worker run");

            var auth = new NetatmoAuth(runnerConfig.NetatmoCreds);
            // for now, run synchronously
            // TODO: change this to run async

            try
            {
                var devices = NetatmoQueries.GetTempAsync(auth.GetToken()).GetAwaiter().GetResult();
                foreach (var device in devices)
                    if (DateTimeOps.IsDataFresh(device.last_status_store))
                    {
                        PostMeasurement(runnerConfig.TempyApiTarget,
                            AssembleMeasurement(device, DataObjects.MeasurementType.Temperature));
                        PostMeasurement(runnerConfig.TempyApiTarget, AssembleMeasurement(device, DataObjects.MeasurementType.Humidity));
                        PostMeasurement(runnerConfig.TempyApiTarget, AssembleMeasurement(device, DataObjects.MeasurementType.CO2));
                    }
                    else
                    {
                        Log.Warning($"Data from device: {device} is more than 20 minutes old. Skipping.");
                    }
            }
            catch (Exception e)
            {
                Log.Warning("Exception encountered during worker run!");
                Log.Debug(e.Message);
            }

            

            stopwatch.Stop();
            Log.Debug($"Worker run took {stopwatch.Elapsed}");
            
        }

        public DataObjects.Measurement AssembleMeasurement(Device device,
            DataObjects.MeasurementType measurementType)
        {
            Log.Debug($"Assembling {measurementType} measurement for device {device}");

            if (device.dashboard_data == null) Log.Warning($"Device: {device} contains empty measurements. Old data?");
            var measurement = new DataObjects.Measurement();

            if (measurementType == DataObjects.MeasurementType.Temperature)
            {
                measurement.Value = (float) device.dashboard_data.Temperature;
                measurement.Type = DataObjects.MeasurementType.Temperature;
            }
            else if (measurementType == DataObjects.MeasurementType.Humidity)
            {
                measurement.Value = device.dashboard_data.Humidity;
                measurement.Type = DataObjects.MeasurementType.Humidity;
            }
            else if (measurementType == DataObjects.MeasurementType.CO2)
            {
                measurement.Value = device.dashboard_data.CO2;
                measurement.Type = DataObjects.MeasurementType.CO2;
            }

            measurement.Name = device.station_name;
            measurement.UnixTimestamp = device.last_status_store;
            //todo: move this to constructor of the object?
            measurement.Source = new DataObjects.Source();
            measurement.Source.Name = device.module_name;
            measurement.Source.Location = $"{device.place.city}, {device.place.country}";

            return measurement;
        }

        public bool CheckApiStatus(RunnerConfig runnerConfig)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri($"http://{runnerConfig.TempyApiTarget}/");
            var request = new RestRequest("status", Method.GET);
            request.RequestFormat = DataFormat.Json;
            Log.Debug($"Checking the health status via {client.BaseUrl}");
            request.AddHeader("Content-Type", "application/json");
            // todo: make the execute async
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {    
                Log.Debug("Health check is successful.");
                return true;
            }
            else
            {
                Log.Warning("Health check failed.");
                return false;

            }
        }

        public bool PostMeasurement(string tempyApiTarget, DataObjects.Measurement measurement)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri($"http://{tempyApiTarget}/");
            var request = new RestRequest("api/measurements", Method.POST);
            request.RequestFormat = DataFormat.Json;
            var json = JsonConvert.SerializeObject(measurement);
            Log.Debug($"Posting measurement to {tempyApiTarget}");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            // todo: make the execute async
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Log.Debug($"Post OK");
                return true;
            }
            else
            {
                Log.Warning($"Post FAILED");
                return false;

            }            
        }
    }
}