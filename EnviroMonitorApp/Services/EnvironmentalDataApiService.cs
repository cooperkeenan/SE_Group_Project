using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services.Apis;

namespace EnviroMonitorApp.Services
{
    public class EnvironmentalDataApiService : IEnvironmentalDataService
    {
        readonly IAirQualityApi _airApi;
        readonly IWeatherApi    _weatherApi;
        readonly ApiKeyProvider _keys;
        readonly IWaterQualityApi _waterApi;


        private static DateTime _airCacheStamp  = DateTime.MinValue;
        private static List<AirQualityRecord>? _airCache;

        private static DateTime _wxCacheStamp   = DateTime.MinValue;
        private static List<WeatherRecord>?    _wxCache;

        public EnvironmentalDataApiService(
            IAirQualityApi airApi,
            IWeatherApi weatherApi,
            IWaterQualityApi waterApi,
            ApiKeyProvider keys)
        {
            _airApi = airApi;
            _weatherApi = weatherApi;
            _waterApi = waterApi;
            _keys = keys;
        }


        public async Task<List<AirQualityRecord>> GetAirQualityAsync()
        {
            if (_airCache != null && (DateTime.UtcNow - _airCacheStamp) < TimeSpan.FromMinutes(10))
                return _airCache;

            var records = new List<AirQualityRecord>();

            try
            {
                var locResp = await _airApi.GetLocations(
                    iso:             "GB",
                    latlon:          "51.5074,-0.1278",
                    radiusMeters:    25_000,
                    parameterIdsCsv: "7,9,2,1",
                    limit:           10 // reduced to avoid API throttling
                );

                foreach (var loc in locResp.Results)
                {
                    var latest = await _airApi.GetLocationLatest(loc.Id);

                    double no2 = 0, so2 = 0, pm25 = 0, pm10 = 0;
                    DateTime stamp = DateTime.MinValue;

                    foreach (var m in latest.Results)
                    {
                        var dt = DateTime.Parse(m.Datetime.Utc, CultureInfo.InvariantCulture);
                        stamp  = dt > stamp ? dt : stamp;

                        var sensor = loc.Sensors.FirstOrDefault(s => s.Id == m.SensorsId);
                        if (sensor is null) continue;

                        switch (sensor.Parameter.Name.ToLowerInvariant())
                        {
                            case "no2":  no2  = m.Value; break;
                            case "so2":  so2  = m.Value; break;
                            case "pm25": pm25 = m.Value; break;
                            case "pm10": pm10 = m.Value; break;
                        }
                    }

                    records.Add(new AirQualityRecord
                    {
                        Timestamp = stamp,
                        NO2       = no2,
                        SO2       = so2,
                        PM25      = pm25,
                        PM10      = pm10
                    });
                }

                _airCache = records;
                _airCacheStamp = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            return records;
        }

        public async Task<List<WeatherRecord>> GetWeatherAsync()
        {
            if (_wxCache != null && (DateTime.UtcNow - _wxCacheStamp) < TimeSpan.FromMinutes(10))
                return _wxCache;

            var resp = await _weatherApi.GetForecast(
                lat:    51.5074,
                lon:   -0.1278,
                apiKey: _keys.OpenWeatherMap,
                units:  "metric"
            );

            var list = resp.List
                .Select(it => new WeatherRecord
                {
                    Timestamp   = DateTimeOffset.FromUnixTimeSeconds(it.Dt).DateTime,
                    Temperature = it.Main.Temp,
                    Humidity    = it.Main.Humidity,
                    WindSpeed   = it.Wind.Speed
                })
                .ToList();

            _wxCache = list;
            _wxCacheStamp = DateTime.UtcNow;

            return list;
        }

        public async Task<IReadOnlyList<WaterQualityRecord>> GetWaterQualityAsync(int hours = 24)
        {
            var since = DateTime.UtcNow.AddHours(-hours).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var resp = await _waterApi.GetRange(
                measureUrl: "https://environment.data.gov.uk/hydrology/id/measures/E05962A-nitrate-i-subdaily-mgL",
                sinceUtc:   since
            );

            return resp.Items.Select(r => new WaterQualityRecord
            {
                Timestamp = DateTime.Parse(r.DateTime),
                Nitrate   = r.Value
            }).ToList();
        }
        public Task<IReadOnlyList<WaterQualityRecord>> GetWaterQualityAsync()
        {
            // default to the last 24 h
            return GetWaterQualityAsync(24);
        }
    }
}
