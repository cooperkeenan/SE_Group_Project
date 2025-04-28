// EnviroMonitorApp/Services/EnvironmentalDataApiService.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services.Apis;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Fetches and paginates historical environmental data from OpenAQ, OpenWeatherMap, and
    /// UK Hydrology, pivoting into our record models.
    /// </summary>
    public class EnvironmentalDataApiService : IEnvironmentalDataService
    {
        private readonly IAirQualityApi   _airApi;
        private readonly IWeatherApi      _weatherApi;
        private readonly IWaterQualityApi _waterApi;
        private readonly ApiKeyProvider   _keys;

        // 10-minute in-memory caches
        private static DateTime _airStamp = DateTime.MinValue;
        private static List<AirQualityRecord>?    _airCache;
        private static DateTime _wxStamp  = DateTime.MinValue;
        private static List<WeatherRecord>?       _wxCache;

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

        // ───────────────────────────────────────────────────────────────────
        // 1) AIR: sensors/{id}/measurements → AirQualityRecord
        // ───────────────────────────────────────────────────────────────────
        public async Task<List<AirQualityRecord>> 
            GetAirQualityAsync(DateTime from, DateTime to, string region)
        {
            // cache slice if fresh
            if (_airCache != null &&
                (DateTime.UtcNow - _airStamp) < TimeSpan.FromMinutes(10))
            {
                return _airCache
                    .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                    .ToList();
            }

            // region → coords stub
            var (iso, coords) = region switch
            {
                "London" => ("GB", "51.5074,-0.1278"),
                _        => throw new ArgumentException($"Unknown region '{region}'")
            };

            // find locations
            var locResp = await _airApi.GetLocations(
                iso: iso,
                latlon: coords,
                radiusMeters: 25_000,
                parameterIdsCsv: "2,1,7,9",  // parameter IDs
                limit: 10
            );

            var allMeas = new List<Measurement>();
            foreach (var loc in locResp.Results)
            {
                foreach (var sensor in loc.Sensors)
                {
                    int page = 1, foundCount;
                    do
                    {
                        var resp = await _airApi.GetSensorMeasurementsAsync(
                            sensor.Id,
                            from.ToString("O", CultureInfo.InvariantCulture),
                            to  .ToString("O", CultureInfo.InvariantCulture),
                            limit: 100,
                            page:  page
                        );

                        if (resp?.Results != null)
                            allMeas.AddRange(resp.Results);

                        // parse found (could be ">100" or number)
                        string raw = resp?.Meta.Found.ValueKind switch
                        {
                            JsonValueKind.String => resp.Meta.Found.GetString()!,
                            JsonValueKind.Number => resp.Meta.Found.GetRawText(),
                            _                    => "0"
                        };
                        if (raw.StartsWith(">")) raw = raw.Substring(1);
                        foundCount = int.TryParse(raw, out var n) ? n : 0;

                        page++;
                    }
                    while ((page - 1) * 100 < foundCount);
                }
            }

            // pivot by measurement.period.datetimeFrom.utc
            var records = allMeas
                .Where(m => m.Period?.DatetimeFrom?.Utc != null)
                .GroupBy(m =>
                {
                    // parse the UTC timestamp
                    var utcStr = m.Period.DatetimeFrom.Utc;
                    return DateTime.Parse(utcStr, CultureInfo.InvariantCulture,
                                          DateTimeStyles.AdjustToUniversal);
                })
                .Select(grp =>
                {
                    var rec = new AirQualityRecord { Timestamp = grp.Key };
                    foreach (var m in grp)
                    {
                        var param = m.Parameter?.Name?.ToLowerInvariant();
                        if (param == null) continue;
                        switch (param)
                        {
                            case "no2":  rec.NO2  = m.Value; break;
                            case "so2":  rec.SO2  = m.Value; break;
                            case "pm25": rec.PM25 = m.Value; break;
                            case "pm10": rec.PM10 = m.Value; break;
                        }
                    }
                    return rec;
                })
                .OrderBy(r => r.Timestamp)
                .ToList();

            _airCache = records;
            _airStamp = DateTime.UtcNow;
            return records;
        }

        // ───────────────────────────────────────────────────────────────────
        // 2) WEATHER: OpenWeatherMap 3-hour forecast → WeatherRecord
        // ───────────────────────────────────────────────────────────────────
        public async Task<List<WeatherRecord>> 
            GetWeatherAsync(DateTime from, DateTime to, string region)
        {
            if (_wxCache != null &&
                (DateTime.UtcNow - _wxStamp) < TimeSpan.FromMinutes(10))
            {
                return _wxCache
                    .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                    .ToList();
            }

            // hardcode London, change to dynamic if needed
            var resp = await _weatherApi.GetForecast(
                lat:   51.5074,
                lon:  -0.1278,
                apiKey: _keys.OpenWeatherMap,
                units:  "metric"
            );

            var list = resp.List
                .Where(item => item != null)
                .Select(r => new WeatherRecord
                {
                    Timestamp   = DateTimeOffset
                                    .FromUnixTimeSeconds(r.Dt)
                                    .UtcDateTime,
                    Temperature = r.Main.Temp,
                    Humidity    = r.Main.Humidity,
                    WindSpeed   = r.Wind.Speed
                })
                .ToList();

            _wxCache = list;
            _wxStamp = DateTime.UtcNow;
            return list
                .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                .ToList();
        }

        // ───────────────────────────────────────────────────────────────────
        // 3) WATER: UK Hydrology sub-daily → WaterQualityRecord
        // ───────────────────────────────────────────────────────────────────
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
            var hours = (int)Math.Ceiling((to - from).TotalHours);
            var data  = await GetWaterQualityAsync(hours, region);
            return data
                .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                .ToList();
        }

        public async Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(int hours, string region = "")
        {
            var since = DateTime.UtcNow
                           .AddHours(-hours)
                           .ToString("yyyy-MM-ddTHH:mm:ssZ",
                                      CultureInfo.InvariantCulture);

            var tasks = _measureMap
                .Select(kvp => _waterApi.GetRange(kvp.Value, since)
                                        .ContinueWith(t => (kvp.Key, t.Result)))
                .ToArray();

            await Task.WhenAll(tasks);

            var bucket = new Dictionary<DateTime,WaterQualityRecord>();
            foreach (var (key, resp) in tasks.Select(t => t.Result))
            {
                if (resp?.Items == null) continue;
                foreach (var r in resp.Items.Take(100))
                {
                    if (!DateTime.TryParse(r.DateTime, out var dt)) 
                        continue;
                    var ts = dt.AddSeconds(-dt.Second);

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

            return bucket.Values
                         .OrderByDescending(r => r.Timestamp)
                         .Take(10)
                         .ToList();
        }
    }
}
