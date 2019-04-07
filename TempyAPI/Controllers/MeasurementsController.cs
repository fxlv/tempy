﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;


namespace TempyAPI.Controllers

{
    [EnableCors("AllowAnyOriginPolicy")]
    [Route("status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        // GET /status
        [HttpGet]
        public string GetStatus()
        {
            Program.LogHttpRequest(Request.Method.ToString(), Response.StatusCode.ToString(), HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
            return "OK";
        }
    }
    
    [Route("api/measurements")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        private readonly TempyDbAuthCredentials _cosmosDbAuthSettings;

        private readonly IHostingEnvironment _hostingEnvironment;

        public MeasurementsController(IHostingEnvironment hostingEnvironment, IConfiguration configuration,
            TempyDbAuthCredentials cosmosDbAuthSettings)
        {
            _hostingEnvironment = hostingEnvironment;
            _cosmosDbAuthSettings = cosmosDbAuthSettings;
        }

    
        // GET api/measurements
        [HttpGet]
        public JsonResult GetAllMeasurementst()
        {
            //TODO: don't create a new cosmosdb connection every time
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetTemperatureMeasurements();
            Program.LogHttpRequest(Request.Method.ToString(), Response.StatusCode.ToString(), HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
            return new JsonResult(result);
        }

        // GET api/measurements/name/<>
        [HttpGet("name/{name}")]
        public JsonResult GetLastMeasurement(string name)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetLatestTemperatureMeasurementByName(name);
            Program.LogHttpRequest(Request.Method.ToString(), Response.StatusCode.ToString(), HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());

            return new JsonResult(result);
        }

        // POST api/measurements
        [HttpPost]
        public string Post([FromBody] DataObjects.Measurement measurement)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            db.WriteDocument(measurement);
            //todo: return only status code
            Program.LogHttpRequest(Request.Method.ToString(), Response.StatusCode.ToString(), HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());

            
            
            return measurement.ToString();
        }
    }
}