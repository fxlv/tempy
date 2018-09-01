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
            // TODO: convert timestamp to datetime upon SET
            public int UnixTimestamp { get; set; }
        }
    
        public class TemperatureMeasurement : Measurement
        {
            public TemperatureSource Source { get; set; }
            public float Value { get; set; }
        }
    }
}