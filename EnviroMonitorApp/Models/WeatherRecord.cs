namespace EnviroMonitorApp.Models
{
    public class WeatherRecord
    {
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
}
