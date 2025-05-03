namespace EnviroMonitorApp.Models
{
    public class AirQualityRecord
    {
        public DateTime Timestamp { get; set; }
        public double NO2 { get; set; }
        public double SO2 { get; set; }
        public double PM25 { get; set; }
        public double PM10 { get; set; }
    }
}
