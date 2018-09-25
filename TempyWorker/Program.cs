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

                var measurement = new TempyAPI.DataObjects.TemperatureMeasurement();
                measurement.Name = device.station_name;
                measurement.Value = (int) device.dashboard_data.Temperature;
                measurement.UnixTimestamp = device.last_status_store;
                measurement.Source = new DataObjects.TemperatureSource();
                measurement.Source.Name = device.module_name;
                measurement.Source.Location = $"{device.place.city}, {device.place.country}";
                measurement.Source.Type = 0;
                
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
            // do netatmo auth
            // initializes netatmolib
            // queries netatmo for data about all sensors
            // iterates over result
            // for each result:
            // send result to tempy API 
            sleepSeconds = sleepSeconds * 1000;
            Thread.Sleep(sleepSeconds);

        }
    }
}