using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    public interface IEnvironmentalDataService
    {
        Task<List<AirQualityRecord>> GetAirQualityAsync();
        Task<List<WeatherRecord>>    GetWeatherAsync();
    }
}
