// IEnvironmentalDataService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

public interface IEnvironmentalDataService
{
  Task<List<WeatherRecord>> GetWeatherAsync();
  Task<List<AirQualityRecord>> GetAirQualityAsync();
  Task<List<WaterQualityRecord>> GetWaterQualityAsync();
}
