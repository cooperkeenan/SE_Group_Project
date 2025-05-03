// EnviroMonitorApp/Models/WeatherRecord.cs
using EnviroMonitorApp.Models;
using System;

namespace EnviroMonitorApp.Models
{
    public class WeatherRecord
    {
        public DateTime Timestamp       { get; set; }

        // API 5-day forecast fields
        public double Temperature       { get; set; }
        public double Humidity          { get; set; }
        public double WindSpeed         { get; set; }

        // CSV-backfill fields
        public double CloudCover        { get; set; }
        public double Sunshine          { get; set; }
        public double GlobalRadiation   { get; set; }
        public double MaxTemp           { get; set; }
        public double MeanTemp          { get; set; }
        public double MinTemp           { get; set; }
        public double Precipitation     { get; set; }
        public double Pressure          { get; set; }
        public double SnowDepth         { get; set; }
    }
}
