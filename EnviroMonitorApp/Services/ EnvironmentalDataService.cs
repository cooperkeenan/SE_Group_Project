// Services/EnvironmentalDataService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
  public class EnvironmentalDataService : IEnvironmentalDataService
  {
    readonly ExcelReaderService _reader;
    public EnvironmentalDataService(ExcelReaderService reader)
      => _reader = reader;

    public Task<List<WeatherRecord>> GetWeatherAsync()
      => Task.Run(() => _reader.ReadWeather("Weather.xlsx"));

    public Task<List<AirQualityRecord>> GetAirQualityAsync()
      => Task.Run(() => _reader.ReadAirQuality("Air_quality.xlsx"));

    public Task<List<WaterQualityRecord>> GetWaterQualityAsync()
      => Task.Run(() => _reader.ReadWaterQuality("Water_quality.xlsx"));
  }
}
