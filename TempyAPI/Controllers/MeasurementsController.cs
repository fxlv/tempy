using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace TempyAPI.Controllers

{
    [Route("api/measurements")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {

        private readonly IHostingEnvironment _hostingEnvironment;

        public MeasurementsController(IHostingEnvironment hostingEnvironment){
            _hostingEnvironment = hostingEnvironment;
        }

        // GET api/measurements
        [HttpGet]
        public JsonResult GetAllMeasurementst()
        {
            var s = new Settings(_hostingEnvironment.ContentRootPath);
            var db = new TempyDB(s);
            var result = db.GetTemperatureMeasurements();
            return new JsonResult(result);
        }

        // GET api/measurements/name/<>
        [HttpGet("name/{name}")]
        public JsonResult GetLastMeasurement(string name)
        {
            var s = new Settings(_hostingEnvironment.ContentRootPath);

            var db = new TempyDB(s);
            var result = db.GetLatestTemperatureMeasurementByName(name);
            return new JsonResult(result);
        }

        // POST api/measurements
        [HttpPost]
        public string Post([FromBody] DataObjects.Measurement measurement)
        {
            var s = new Settings(_hostingEnvironment.ContentRootPath);

            var db = new TempyDB(s);
            db.WriteDocument(measurement);
            //todo: return only status code
            return measurement.ToString();
        }
    }
}