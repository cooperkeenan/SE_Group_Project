namespace EnviroMonitorApp.Models
{
    public class AirQualityRecord
    {
        public DateTime Timestamp { get; set; }

        public double? NO2 { get; set; }   // Nitrogen Dioxide
        public double? SO2 { get; set; }   // Sulfur Dioxide
        public double? PM25 { get; set; }  // Particulate Matter 2.5
        public double? PM10 { get; set; }  // Particulate Matter 10
        public double? Pm25 { get; set; }  // Same as PM25, probably a typo or duplication
        public double? Pm10 { get; set; }  // Same as PM10, probably a typo or duplication
        public string Category { get; set; } // Air quality category like "Good", "Moderate", etc.
    }
}
