// EnviroMonitorApp/Models/OpenWeatherForecastResponse.cs
using System;
using System.Collections.Generic;

namespace EnviroMonitorApp.Models
{
    public class OpenWeatherForecastResponse
    {
        // The API returns an array called "list"
        public List<ListItem> List { get; set; } = new List<ListItem>();

        public class ListItem
        {
            public long Dt { get; set; }
            public MainInfo Main { get; set; }
            public WindInfo Wind { get; set; }
        }

        public class MainInfo
        {
            public double Temp { get; set; }
            public double Humidity { get; set; }
        }

        public class WindInfo
        {
            public double Speed { get; set; }
        }
    }
}
