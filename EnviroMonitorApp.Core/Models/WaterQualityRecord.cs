namespace EnviroMonitorApp.Models
{
    public class WaterQualityRecord
    {
        public DateTime Timestamp       { get; set; }

        public double? Nitrate          { get; set; }
        public double? PH               { get; set; }
        public double? DissolvedOxygen  { get; set; } // mg/L
        public double? Temperature      { get; set; } // °C
    }
}
