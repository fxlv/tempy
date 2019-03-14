using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace TempyAPI.Controllers

{
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
            Log.Debug($"Status: {Response.StatusCode} Remote IP: {HttpContext.Connection.RemoteIpAddress} Path: {HttpContext.Request.Path} ");
            return new JsonResult(result);
        }

        // GET api/measurements/name/<>
        [HttpGet("name/{name}")]
        public JsonResult GetLastMeasurement(string name)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            var result = db.GetLatestTemperatureMeasurementByName(name);
            Log.Debug($"Status: {Response.StatusCode} Remote IP: {HttpContext.Connection.RemoteIpAddress} Path: {HttpContext.Request.Path} ");

            return new JsonResult(result);
        }

        // POST api/measurements
        [HttpPost]
        public string Post([FromBody] DataObjects.Measurement measurement)
        {
            var db = new TempyDB(_cosmosDbAuthSettings);
            db.WriteDocument(measurement);
            //todo: return only status code
            Log.Debug($"Status: {Response.StatusCode} Remote IP: {HttpContext.Connection.RemoteIpAddress} Path: {HttpContext.Request.Path} ");

            return measurement.ToString();
        }
    }
}