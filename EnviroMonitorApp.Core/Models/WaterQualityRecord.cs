namespace EnviroMonitorApp.Models
{
    /// <summary>
    /// Represents a record of water quality measurements at a specific point in time.
    /// Contains various water quality parameters including nitrate, pH, dissolved oxygen, and temperature.
    /// </summary>
    public class WaterQualityRecord
    {
        /// <summary>
        /// The date and time when the water quality measurements were recorded.
        /// </summary>
        public DateTime Timestamp       { get; set; }

        /// <summary>
        /// Nitrate concentration in milligrams per liter (mg/L).
        /// </summary>
        public double? Nitrate          { get; set; }
        
        /// <summary>
        /// pH value indicating the acidity or alkalinity of the water (scale of 0-14).
        /// </summary>
        public double? PH               { get; set; }
        
        /// <summary>
        /// Dissolved oxygen level in milligrams per liter (mg/L).
        /// </summary>
        public double? DissolvedOxygen  { get; set; }
        
        /// <summary>
        /// Water temperature in degrees Celsius (°C).
        /// </summary>
        public double? Temperature      { get; set; }
    }
}