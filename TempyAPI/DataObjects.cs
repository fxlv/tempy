using System;
using System.Linq;
using Newtonsoft.Json;

namespace TempyAPI
{
    public class DataObjects
    {
        public enum SourceType
        {
            Temperature,
            Humidity,
            CO2,
            Noise
        }

        public class Source
        {
            public string Name { get; set; }
            public SourceType Type { get; set; }
            public string Location { get; set; }

        }

   

        public class Measurement
        {
            [JsonProperty(PropertyName = "id")] public string Id { set; get; }

            public string Name { get; set; }
            public Source Source { get; set; }
            public float Value { get; set; }

            // TODO: convert timestamp to datetime upon SET?
            public int UnixTimestamp { get; set; }
            
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
            
       
        }

        public class HumidityMeasurement : Measurement
        {

       
        }
        
        public class TemperatureMeasurement : Measurement
        {
            
        }
    }
}