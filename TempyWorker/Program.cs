using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.VisualBasic.CompilerServices;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using NetatmoLib;
using Newtonsoft.Json;
using RestSharp;
using TempyAPI;

namespace TempyWorker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // initialize logging
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .Console(LogEventLevel.Information, theme: ConsoleTheme.None)
                .WriteTo.File("worker.log", rollingInterval: RollingInterval.Day).CreateLogger();
            Log.Debug("logging started");
            Console.Title = "Tempy Worker";
            WorkerRunner();
            // go into loop and constantly (configurable sleep interval) call worker()


        }

        public static void WorkerRunner()
        {
            while (true)
            {
                Worker();
            }
        }
        
        
        async public static void  Worker(int sleepSeconds = 300){

            // do netatmo auth
            // initializes netatmolib
            // queries netatmo for data about all sensors
            // iterates over result
            // for each result:
            // send result to tempy API 

            Log.Debug("Doing worker run");

            var auth = new NetatmoAuth();
            // for now, run synchronously
            // TODO: change this to run async
            var devices = NetatmoQueries.GetTempAsync(auth.GetToken()).GetAwaiter().GetResult();
            
            foreach (var device in devices)
            {
                PostMeasurement(AssembleMeasurement(device, DataObjects.MeasurementType.Temperature));
                PostMeasurement(AssembleMeasurement(device, DataObjects.MeasurementType.Humidity));
                PostMeasurement(AssembleMeasurement(device, DataObjects.MeasurementType.CO2));
            }
           
            sleepSeconds = sleepSeconds * 1000;
            Thread.Sleep(sleepSeconds);

        }

        public static DataObjects.Measurement AssembleMeasurement(Device device, DataObjects.MeasurementType measurementType)
        {
            Log.Debug($"Assembling {measurementType} measurement for device {device}");
            var measurement = new TempyAPI.DataObjects.Measurement();

            if(measurementType == DataObjects.MeasurementType.Temperature){
                measurement.Value = (float)device.dashboard_data.Temperature;
                measurement.Type = DataObjects.MeasurementType.Temperature;
            } else if (measurementType == DataObjects.MeasurementType.Humidity){
                measurement.Value = (float)device.dashboard_data.Humidity;
                measurement.Type = DataObjects.MeasurementType.Humidity;
            }
            else if (measurementType == DataObjects.MeasurementType.CO2){
                measurement.Value = (float)device.dashboard_data.CO2;
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

        public static void PostMeasurement(DataObjects.Measurement measurement)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("http://localhost:5000/");
            var request = new RestRequest("api/measurements", Method.POST);
            request.RequestFormat = DataFormat.Json;
            var json = JsonConvert.SerializeObject(measurement);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            // todo: make the execute async
            client.Execute(request);
        }
        
        
    }
}