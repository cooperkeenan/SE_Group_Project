// EnviroMonitorApp/Models/WeatherRecord.cs
using EnviroMonitorApp.Models;
using System;

namespace EnviroMonitorApp.Models
{
    /// <summary>
    /// Represents a record of weather measurements at a specific point in time.
    /// Contains various weather parameters from both forecast APIs and historical CSV data.
    /// </summary>
    public class WeatherRecord
    {
        /// <summary>
        /// The date and time when the weather measurements were recorded.
        /// </summary>
        public DateTime Timestamp       { get; set; }

        /// <summary>
        /// Current air temperature in degrees Celsius (°C).
        /// This is typically sourced from the 5-day forecast API.
        /// </summary>
        public double Temperature       { get; set; }
        
        /// <summary>
        /// Relative humidity percentage (0-100).
        /// This is typically sourced from the 5-day forecast API.
        /// </summary>
        public double Humidity          { get; set; }
        
        /// <summary>
        /// Wind speed in meters per second (m/s).
        /// This is typically sourced from the 5-day forecast API.
        /// </summary>
        public double WindSpeed         { get; set; }

        /// <summary>
        /// Cloud cover percentage (0-100).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double CloudCover        { get; set; }
        
        /// <summary>
        /// Sunshine duration in hours.
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double Sunshine          { get; set; }
        
        /// <summary>
        /// Global radiation in watts per square meter (W/m²).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double GlobalRadiation   { get; set; }
        
        /// <summary>
        /// Maximum temperature for the day in degrees Celsius (°C).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double MaxTemp           { get; set; }
        
        /// <summary>
        /// Mean temperature for the day in degrees Celsius (°C).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double MeanTemp          { get; set; }
        
        /// <summary>
        /// Minimum temperature for the day in degrees Celsius (°C).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double MinTemp           { get; set; }
        
        /// <summary>
        /// Precipitation amount in millimeters (mm).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double Precipitation     { get; set; }
        
        /// <summary>
        /// Atmospheric pressure in hectopascals (hPa).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double Pressure          { get; set; }
        
        /// <summary>
        /// Snow depth in centimeters (cm).
        /// This is typically sourced from CSV backfill data.
        /// </summary>
        public double SnowDepth         { get; set; }
    }
}