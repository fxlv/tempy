using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
            Program.LogHttpRequest(Request.Method, Response.StatusCode.ToString(),
                HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
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


        // GET api/names
        [EnableCors("AllowAnyOriginPolicy")]
        [HttpGet("names")]
        public JsonResult GetNames()
        {
            //TODO: don't create a new cosmosdb connection every time
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetNames();
            Program.LogHttpRequest(Request.Method, Response.StatusCode.ToString(),
                HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
            return new JsonResult(result);
        }
        
        // GET api/measurements
        [HttpGet]
        public JsonResult GetAllMeasurements()
        {
            //TODO: don't create a new cosmosdb connection every time
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetTemperatureMeasurements();
            Program.LogHttpRequest(Request.Method, Response.StatusCode.ToString(),
                HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
            return new JsonResult(result);
        }

        // GET api/measurements/<name>
        [EnableCors("AllowAnyOriginPolicy")]
        [HttpGet("{name}")]
        public JsonResult GetLastMeasurement(string name)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetLatestMeasurementByName(name);
            Program.LogHttpRequest(Request.Method, Response.StatusCode.ToString(),
                HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
            var jsonResult = new JsonResult(result);
            
            return jsonResult;
        }
        
        // GET api/measurements/<name>/<type>
        [EnableCors("AllowAnyOriginPolicy")]
        [HttpGet("{name}/{measurementType}")]
        public JsonResult GetLastMeasurementType(string name, int measurementType)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetLatestMeasurementByName(name, measurementType);
            Program.LogHttpRequest(Request.Method, Response.StatusCode.ToString(),
                HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());
            var jsonResult = new JsonResult(result);
            
            return jsonResult;
        }

        // POST api/measurements
        [HttpPost]
        public string Post([FromBody] DataObjects.Measurement measurement)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            db.WriteDocument(measurement);
            //todo: return only status code
            Program.LogHttpRequest(Request.Method, Response.StatusCode.ToString(),
                HttpContext.Connection.RemoteIpAddress.ToString(), HttpContext.Request.Path.ToString());


            return measurement.ToString();
        }
    }
}