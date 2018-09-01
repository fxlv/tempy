using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TempyAPI.Controllers

{
    


    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        // GET api/measurements
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new[] {"value1", "value2"};
        }


        // GET api/measurements/something
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            return $"String value {id}";
        }

        
      
        [HttpPost]
        public string Post([FromBody] DataObjects.TemperatureMeasurement value)
        {
            return value.ToString();
        }
    }
}