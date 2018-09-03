﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace TempyAPI.Controllers

{
    [Route("api/measurements")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        // GET api/measurements
        [HttpGet]
        public JsonResult GetAllMeasurementst()
        {
           var db = new TempyDB();
           var result = db.GetTemperatureMeasurements();
            return new JsonResult(result);
        }

        // GET api/measurements/name/<>
        [HttpGet("name/{name}")]
        public JsonResult GetLastMeasurement(string name)  
        {
            var db = new TempyDB();
            var result = db.GetLatestTemperatureMeasurementByName(name);
            return new JsonResult(result);
            
        }


        [HttpPost]
        public string Post([FromBody] DataObjects.TemperatureMeasurement tempMeasurement)
        {
            var db = new TempyDB();
            db.WriteDocument(tempMeasurement);
            return tempMeasurement.ToString();
        }
    }
}