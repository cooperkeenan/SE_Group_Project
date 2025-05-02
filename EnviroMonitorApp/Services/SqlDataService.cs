// Services/SqlDataService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    public class SqlDataService : IEnvironmentalDataService
    {
        readonly SQLiteAsyncConnection _db;
        const string FileName = "enviro.db3";

        public SqlDataService()
        {
            var folder = FileSystem.AppDataDirectory;
            var dest   = Path.Combine(folder, FileName);
            Debug.WriteLine($"[SqlDataService] Dest DB path: {dest}");

            if (File.Exists(dest))
            {
                Debug.WriteLine($"[SqlDataService] Deleting existing DB");
                File.Delete(dest);
            }

            Debug.WriteLine($"[SqlDataService] Copying bundle DB → {dest}");
            using var src  = FileSystem.OpenAppPackageFileAsync(FileName).Result;
            using var outp = File.Create(dest);
            src.CopyTo(outp);

            _db = new SQLiteAsyncConnection(dest);
            Debug.WriteLine($"[SqlDataService] Opened SQLite DB");

            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();

            var totalAir = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AirQualityRecord").Result;
            Debug.WriteLine($"[SqlDataService] Total rows in AirQualityRecord: {totalAir}");
            var totalWater = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WaterQualityRecord").Result;
            Debug.WriteLine($"[SqlDataService] Total rows in WaterQualityRecord: {totalWater}");
        }

        class RawAir
        {
            public string Timestamp { get; set; } = "";
            public double? NO2     { get; set; }
            public double? SO2     { get; set; }
            public double? PM25    { get; set; }
            public double? PM10    { get; set; }
        }

        public async Task<List<AirQualityRecord>> GetAirQualityAsync(
            DateTime from, DateTime to, string region)
        {
            Debug.WriteLine($"[SqlDataService] Loading raw AirQualityRecord rows");
            var raws = await _db.QueryAsync<RawAir>(
                "SELECT Timestamp, NO2, SO2, PM25, PM10 FROM AirQualityRecord");

            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            var outList = new List<AirQualityRecord>();

            foreach (var r in raws)
            {
                if (!DateTime.TryParseExact(
                        r.Timestamp,
                        "yyyy-MM-dd'T'HH:mm:ss'Z'",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var dt)
                    && !DateTime.TryParse(
                        r.Timestamp,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out dt))
                {
                    Debug.WriteLine($"[SqlDataService] ⚠ could not parse «{r.Timestamp}»");
                    continue;
                }

                if (dt < from || dt > toInclusive)
                    continue;

                outList.Add(new AirQualityRecord
                {
                    Timestamp = dt,
                    NO2       = r.NO2  ?? 0,
                    SO2       = r.SO2  ?? 0,
                    PM25      = r.PM25 ?? 0,
                    PM10      = r.PM10 ?? 0,
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed+filtered Air rows: {outList.Count}");
            return outList;
        }

        public Task<List<WeatherRecord>> GetWeatherAsync(DateTime from, DateTime to, string region)
            => Task.FromResult(new List<WeatherRecord>());

        // ← **NEW**: Real DB lookup for water history
        public async Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            DateTime from, DateTime to, string region)
        {
            Debug.WriteLine($"[SqlDataService] Loading WaterQualityRecord rows");
            var raws = await _db.QueryAsync<WaterQualityRecord>(
                @"SELECT Timestamp as Timestamp,
                         Nitrate, PH, DissolvedOxygen, Temperature
                  FROM WaterQualityRecord");

            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            var outList = new List<WaterQualityRecord>();

            foreach (var r in raws)
            {
                // Parse the TEXT timestamp in the same way as air
                if (!DateTime.TryParseExact(
                        r.Timestamp.ToString("u").Replace(' ', 'T').TrimEnd('Z'),
                        "yyyy-MM-ddTHH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var dt)
                    && !DateTime.TryParse(r.Timestamp.ToString(), null, DateTimeStyles.AdjustToUniversal, out dt))
                {
                    Debug.WriteLine($"[SqlDataService] ⚠ could not parse water ts «{r.Timestamp}»");
                    continue;
                }

                if (dt < from || dt > toInclusive)
                    continue;

                outList.Add(new WaterQualityRecord
                {
                    Timestamp       = dt,
                    Nitrate         = r.Nitrate,
                    PH              = r.PH,
                    DissolvedOxygen = r.DissolvedOxygen,
                    Temperature     = r.Temperature
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed+filtered Water rows: {outList.Count}");
            return outList;
        }

        // Leave this overload stubbed if you still need the API version elsewhere
            // existing stub
        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "")
            => Task.FromResult(new List<WaterQualityRecord>());

        // ← ADD THIS:
        public Task<List<WaterQualityRecord>> GetHistoricalWaterQualityAsync(
            DateTime from, DateTime to, string region)
        {
            return GetWaterQualityAsync(from, to, region);
        }
    }

}
