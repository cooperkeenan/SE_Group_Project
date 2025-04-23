namespace EnviroMonitorApp.Models
{
    public class WaterQualityRecord
    {
        public DateTime Timestamp { get; set; }
        public double Nitrate { get; set; }
        public double Nitrite { get; set; }
        public double Phosphate { get; set; }
        public double EC { get; set; }
    }
}
