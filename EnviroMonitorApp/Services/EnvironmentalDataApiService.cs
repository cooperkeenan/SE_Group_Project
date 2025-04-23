using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services.Apis;

namespace EnviroMonitorApp.Services
{
    public interface IEnvironmentalDataService
    {
        Task<List<AirQualityRecord>>   GetAirQualityAsync();
        Task<List<WeatherRecord>>      GetWeatherAsync();
        Task<List<WaterQualityRecord>> GetWaterQualityAsync();
    }

    public class EnvironmentalDataApiService : IEnvironmentalDataService
    {
        readonly IAirQualityApi    _airApi;
        readonly IWeatherApi       _weatherApi;
        readonly IWaterQualityApi  _waterApi;
        readonly ApiKeyProvider    _keys;

        public EnvironmentalDataApiService(
            IAirQualityApi airApi,
            IWeatherApi weatherApi,
            IWaterQualityApi waterApi,
            ApiKeyProvider keys)
        {
            _airApi     = airApi;
            _weatherApi = weatherApi;
            _waterApi   = waterApi;
            _keys       = keys;
        }

        public async Task<List<AirQualityRecord>> GetAirQualityAsync()
        {
            var raw = await _airApi.GetMeasurements(
                city:       "Edinburgh",
                parameters: "no2,so2,pm25,pm10",
                fromUtc:    DateTime.UtcNow.AddDays(-1).ToString("o"),
                limit:      500);

            return raw.Results
                .Select(r => new AirQualityRecord {
                    Timestamp = DateTime.Parse(r.Date.Utc.Utc),  // <-- use r.Date.Utc.Utc (string)
                    NO2       = r.Parameter == "no2"  ? r.Value : 0,
                    SO2       = r.Parameter == "so2"  ? r.Value : 0,
                    PM25      = r.Parameter == "pm25" ? r.Value : 0,
                    PM10      = r.Parameter == "pm10" ? r.Value : 0
                })
                .ToList();
        }

        public async Task<List<WeatherRecord>> GetWeatherAsync()
        {
            var resp = await _weatherApi.GetForecast(
                lat:    55.94576,
                lon:    -3.184,
                apiKey: _keys.OpenWeatherMap,
                units:  "metric");

            return resp.List
                .Select(item => new WeatherRecord {
                    Timestamp   = DateTimeOffset
                                    .FromUnixTimeSeconds(item.Dt)
                                    .DateTime,
                    Temperature = item.Main.Temp,
                    Humidity    = item.Main.Humidity,
                    WindSpeed   = item.Wind.Speed
                })
                .ToList();
        }

        public async Task<List<WaterQualityRecord>> GetWaterQualityAsync()
        {
            var resp = await _waterApi.Search(
                format:             "json",
                startDate:          DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd"),
                characteristicName: "Nitrate,Nitrite,Phosphate");

            return resp.Results
                .Select(r => new WaterQualityRecord {
                    Timestamp = r.ActivityStartDateTime,
                    Nitrate   = r.CharacteristicName == "Nitrate"   ? r.Value : 0,
                    Nitrite   = r.CharacteristicName == "Nitrite"   ? r.Value : 0,
                    Phosphate = r.CharacteristicName == "Phosphate" ? r.Value : 0,
                    EC        = 0
                })
                .ToList();
        }
    }
}