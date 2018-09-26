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
            
            Log.Debug("doing worker run");
           
            
            
            var auth = new NetatmoAuth();
            // for now, run synchronously
            // TODO: change this to run async
            var devices = NetatmoQueries.GetTempAsync(auth.GetToken()).GetAwaiter().GetResult();
            
            foreach (var device in devices)
            {

                var temperatureMeasurement = new TempyAPI.DataObjects.TemperatureMeasurement();
                temperatureMeasurement.Name = device.station_name;
                temperatureMeasurement.Value = (float) device.dashboard_data.Temperature;
                temperatureMeasurement.UnixTimestamp = device.last_status_store;
                temperatureMeasurement.Source = new DataObjects.Source();
                temperatureMeasurement.Source.Name = device.module_name;
                temperatureMeasurement.Source.Location = $"{device.place.city}, {device.place.country}";
                temperatureMeasurement.Source.Type =  DataObjects.SourceType.Temperature;
                
                var humidityMeasurement = new TempyAPI.DataObjects.HumidityMeasurement();
                humidityMeasurement.Name = device.station_name;
                humidityMeasurement.Value = (float) device.dashboard_data.Humidity;
                humidityMeasurement.UnixTimestamp = device.last_status_store;
                humidityMeasurement.Source = new DataObjects.Source();
                humidityMeasurement.Source.Name = device.module_name;
                humidityMeasurement.Source.Location = $"{device.place.city}, {device.place.country}";
                humidityMeasurement.Source.Type = DataObjects.SourceType.Humidity;
                
                
              
                PostMeasurement(temperatureMeasurement);
                PostMeasurement(humidityMeasurement);

         
                


            }
            // do netatmo auth
            // initializes netatmolib
            // queries netatmo for data about all sensors
            // iterates over result
            // for each result:
            // send result to tempy API 
            sleepSeconds = sleepSeconds * 1000;
            Thread.Sleep(sleepSeconds);

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