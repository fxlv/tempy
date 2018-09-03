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
        }

        public class TemperatureSource : Source
        {
            public string Location { get; set; }
        }

        public class Measurement
        {
            public string Name { get; set; }

            // TODO: convert timestamp to datetime upon SET?
            public int UnixTimestamp { get; set; }
        }

        public class TemperatureMeasurement : Measurement
        {
            [JsonProperty(PropertyName = "id")] public string Id { set; get; }
            public TemperatureSource Source { get; set; }
            public float Value { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
}