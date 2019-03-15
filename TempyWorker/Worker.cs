using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NetatmoLib;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using TempyAPI;

namespace TempyWorker
{
    public class Worker
    {
        public NetatmoApiAuthCredentials GetNetatmoApiAuthCredentials(IConfigurationRoot configuration)
        {
            // instantiate netatmoCreds object that will keep our credentials
            var netatmoCreds = new NetatmoApiAuthCredentials();
            // populate the credentials from the configuration
            configuration.GetSection("netatmo_api_auth").Bind(netatmoCreds);
            return netatmoCreds;
        }

        public void Run()
        {
            var tempyConfiguration = new TempyConfiguration();
            var configuration = tempyConfiguration.GetConfigurationRoot();
            var netatmoCreds = GetNetatmoApiAuthCredentials(configuration);

            var tempyApiTarget = Environment.GetEnvironmentVariable("TEMPY_API_TARGET");
            if (tempyApiTarget == null) tempyApiTarget = "localhost:5000";

            // go into loop and constantly (configurable sleep interval) call worker()
            while (true) WorkerRunner(netatmoCreds, tempyApiTarget);
        }


        public async void WorkerRunner(NetatmoApiAuthCredentials netatmoCreds, string tempyApiTarget,
            int sleepSeconds = 300)
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

            var auth = new NetatmoAuth(netatmoCreds);
            // for now, run synchronously
            // TODO: change this to run async
            var devices = NetatmoQueries.GetTempAsync(auth.GetToken()).GetAwaiter().GetResult();

            foreach (var device in devices)
                if (DateTimeOps.IsDataFresh(device.last_status_store))
                {
                    PostMeasurement(tempyApiTarget,
                        AssembleMeasurement(device, DataObjects.MeasurementType.Temperature));
                    PostMeasurement(tempyApiTarget, AssembleMeasurement(device, DataObjects.MeasurementType.Humidity));
                    PostMeasurement(tempyApiTarget, AssembleMeasurement(device, DataObjects.MeasurementType.CO2));
                }
                else
                {
                    Log.Warning($"Data from device: {device} is more than 20 minutes old. Skipping.");
                }

            stopwatch.Stop();
            Log.Debug($"Worker run took {stopwatch.Elapsed}");

            sleepSeconds = sleepSeconds * 1000;
            Thread.Sleep(sleepSeconds);
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

        public void PostMeasurement(string tempyApiTarget, DataObjects.Measurement measurement)
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
            client.Execute(request);
            // todo: check and return result of the POST query
        }
    }
}