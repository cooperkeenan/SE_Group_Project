namespace EnviroMonitorApp.Models
{
    /// <summary>
    /// Represents a record of air quality measurements at a specific point in time.
    /// Contains various pollutant measurements and an overall air quality category.
    /// </summary>
    public class AirQualityRecord
    {
        /// <summary>
        /// The date and time when the air quality measurements were recorded.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Nitrogen Dioxide concentration in micrograms per cubic meter (μg/m³).
        /// </summary>
        public double? NO2 { get; set; }   
        
        /// <summary>
        /// Sulfur Dioxide concentration in micrograms per cubic meter (μg/m³).
        /// </summary>
        public double? SO2 { get; set; }   
        
        /// <summary>
        /// Particulate Matter 2.5 concentration in micrograms per cubic meter (μg/m³).
        /// Particles with diameter less than 2.5 micrometers.
        /// </summary>
        public double? PM25 { get; set; }  
        
        /// <summary>
        /// Particulate Matter 10 concentration in micrograms per cubic meter (μg/m³).
        /// Particles with diameter less than 10 micrometers.
        /// </summary>
        public double? PM10 { get; set; }  
        
        /// <summary>
        /// Alternative property for PM2.5 measurement (maintained for compatibility).
        /// </summary>
        public double? Pm25 { get; set; }  
        
        /// <summary>
        /// Alternative property for PM10 measurement (maintained for compatibility).
        /// </summary>
        public double? Pm10 { get; set; }  
        
        /// <summary>
        /// Air quality category description (e.g., "Good", "Moderate", "Poor").
        /// </summary>
        public string Category { get; set; } 
    }
}