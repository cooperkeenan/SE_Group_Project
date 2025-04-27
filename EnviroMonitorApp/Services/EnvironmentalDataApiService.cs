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
        // injected Refit clients + key provider
        private readonly IAirQualityApi   _airApi;
        private readonly IWeatherApi      _weatherApi;
        private readonly IWaterQualityApi _waterApi;
        private readonly ApiKeyProvider   _keys;

        // simple in-memory caches
        private static DateTime _airStamp = DateTime.MinValue;
        private static List<AirQualityRecord>? _airCache;

        private static DateTime _wxStamp  = DateTime.MinValue;
        private static List<WeatherRecord>? _wxCache;

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

        // ─────────────────────── AIR ────────────────────────────
        public async Task<List<AirQualityRecord>> 
            GetAirQualityAsync(DateTime from, DateTime to, string region)
        {
            // Cache for 10m
            if (_airCache != null && (DateTime.UtcNow - _airStamp) < TimeSpan.FromMinutes(10))
            {
                return _airCache
                    .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                    .ToList();
            }

            var outList = new List<AirQualityRecord>();
            var locs = await _airApi.GetLocations(
                           iso:"GB",
                           latlon:"51.5074,-0.1278",
                           radiusMeters:25_000,
                           parameterIdsCsv:"7,9,2,1",
                           limit:10);

            foreach (var l in locs.Results)
            {
                var latest = await _airApi.GetLocationLatest(l.Id);

                double no2=0, so2=0, pm25=0, pm10=0; 
                DateTime ts = DateTime.MinValue;

                foreach (var m in latest.Results)
                {
                    var recorded = DateTime.Parse(m.Datetime.Utc, CultureInfo.InvariantCulture);
                    if (recorded > ts)
                        ts = recorded;

                    var sensor = l.Sensors.FirstOrDefault(s => s.Id == m.SensorsId);
                    if (sensor == null) continue;

                    switch(sensor.Parameter.Name.ToLower())
                    {
                        case "no2":  no2  = m.Value; break;
                        case "so2":  so2  = m.Value; break;
                        case "pm25": pm25 = m.Value; break;
                        case "pm10": pm10 = m.Value; break;
                    }
                }

                outList.Add(new AirQualityRecord
                {
                    Timestamp = ts,
                    NO2       = no2,
                    SO2       = so2,
                    PM25      = pm25,
                    PM10      = pm10
                });
            }

            _airCache = outList;
            _airStamp = DateTime.UtcNow;

            // apply from/to filter
            return outList
                .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                .ToList();
        }

        // ─────────────────────── WEATHER ───────────────────────
        public async Task<List<WeatherRecord>> 
            GetWeatherAsync(DateTime from, DateTime to, string region)
        {
            if (_wxCache != null && (DateTime.UtcNow - _wxStamp) < TimeSpan.FromMinutes(10))
                return _wxCache.Where(r => r.Timestamp >= from && r.Timestamp <= to).ToList();

            // ← here’s the changed call:
            var resp = await _weatherApi.GetForecast(
                51.5074,
                -0.1278,
                _keys.OpenWeatherMap,  // ← positional API key
                "metric");

            var list = resp.List.Select(r => new WeatherRecord {
                Timestamp   = DateTimeOffset.FromUnixTimeSeconds(r.Dt).UtcDateTime,
                Temperature = r.Main.Temp,
                Humidity    = r.Main.Humidity,
                WindSpeed   = r.Wind.Speed
            }).ToList();

            _wxCache = list;
            _wxStamp = DateTime.UtcNow;

            return list.Where(r => r.Timestamp >= from && r.Timestamp <= to).ToList();
        }


        // ─────────────────────── WATER ────────────────────────

        // Base URLs for each parameter
        private static readonly Dictionary<string,string> _measureMap = new()
        {
            ["nitrate"] = "https://environment.data.gov.uk/hydrology/id/measures/E05962A-nitrate-i-subdaily-mgL",
            ["ph"]      = "https://environment.data.gov.uk/hydrology/id/measures/E05962A-ph-i-subdaily",
            ["oxygen"]  = "https://environment.data.gov.uk/hydrology/id/measures/E05962A-do-i-subdaily-mgL",
            ["temp"]    = "https://environment.data.gov.uk/hydrology/id/measures/E05962A-temp-i-subdaily-C"
        };

        public async Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(DateTime from, DateTime to, string region)
        {
            // compute hours span
            var span = to - from;
            var hours = (int)Math.Ceiling(span.TotalHours);

            var list = await GetWaterQualityAsync(hours, region);
            return list
                .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                .ToList();
        }

        public async Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(int hours, string region = "")
        {
            var since = DateTime.UtcNow
                           .AddHours(-hours)
                           .ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            // fetch each parameter in parallel
            var tasks = _measureMap.Select(kvp =>
                _waterApi.GetRange(kvp.Value, since)
                         .ContinueWith(t => (kvp.Key, t.Result)))
                .ToArray();

            await Task.WhenAll(tasks);

            // merge into time-bucketed records
            var bucket = new Dictionary<DateTime,WaterQualityRecord>();
            foreach (var (key, resp) in tasks.Select(t => t.Result))
            {
                foreach (var r in resp.Items.Take(100))  // cap
                {
                    var ts = DateTime.Parse(r.DateTime, CultureInfo.InvariantCulture)
                                      .AddSeconds(-DateTime.Parse(r.DateTime).Second);

                    if (!bucket.TryGetValue(ts, out var rec))
                        bucket[ts] = rec = new WaterQualityRecord { Timestamp = ts };

                    switch (key)
                    {
                        case "nitrate":   rec.Nitrate         = r.Value; break;
                        case "ph":        rec.PH              = r.Value; break;
                        case "oxygen":    rec.DissolvedOxygen = r.Value; break;
                        case "temp":      rec.Temperature     = r.Value; break;
                    }
                }
            }

            // return newest 10
            return bucket.Values
                         .OrderByDescending(r => r.Timestamp)
                         .Take(10)
                         .ToList();
        }
    }
}
