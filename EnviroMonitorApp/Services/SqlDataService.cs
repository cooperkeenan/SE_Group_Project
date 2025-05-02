// EnviroMonitorApp/Services/SqlDataService.cs
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
            // 1) Copy prebuilt DB into AppData
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

            // 2) Open SQLite connection
            _db = new SQLiteAsyncConnection(dest);
            Debug.WriteLine($"[SqlDataService] Opened SQLite DB at {dest}");

            // 3) Ensure tables exist
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();

            // 4) Log row counts
            var totalAir   = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AirQualityRecord").Result;
            var totalWater = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WaterQualityRecord").Result;
            Debug.WriteLine($"[SqlDataService] AirQualityRecord rows: {totalAir}");
            Debug.WriteLine($"[SqlDataService] WaterQualityRecord rows: {totalWater}");
        }

        // DTO for AirQualityRecord raw query
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
            Debug.WriteLine("[SqlDataService] Loading AirQualityRecord rows");
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
                    Debug.WriteLine($"[SqlDataService] ⚠ Could not parse air ts «{r.Timestamp}»");
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

        // Weather is stubbed for now
        public Task<List<WeatherRecord>> GetWeatherAsync(DateTime from, DateTime to, string region)
            => Task.FromResult(new List<WeatherRecord>());

        // DTO for WaterQualityRecord raw query
        class RawWater
        {
            public string Timestamp { get; set; } = "";
            public double? Nitrate          { get; set; }
            public double? PH               { get; set; }
            public double? DissolvedOxygen  { get; set; }
            public double? Temperature      { get; set; }
        }

        public async Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading WaterQualityRecord rows");

            // Build ISO bounds
            var fromIso = from.ToString("yyyy-MM-dd") + "T00:00:00Z";
            var toIso   = to  .ToString("yyyy-MM-dd") + "T23:59:59Z";

            var sql = @"
              SELECT Timestamp, Nitrate, PH, DissolvedOxygen, Temperature
                FROM WaterQualityRecord
               WHERE Timestamp >= ? AND Timestamp <= ?
               ORDER BY Timestamp";

            var raws = await _db.QueryAsync<RawWater>(sql, fromIso, toIso);
            var outList = new List<WaterQualityRecord>();

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
                    Debug.WriteLine($"[SqlDataService] ⚠ Could not parse water ts «{r.Timestamp}»");
                    continue;
                }

                outList.Add(new WaterQualityRecord
                {
                    Timestamp       = dt,
                    Nitrate         = r.Nitrate,
                    PH              = r.PH,
                    DissolvedOxygen = r.DissolvedOxygen,
                    Temperature     = r.Temperature
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed water rows: {outList.Count}");
            return outList;
        }

        // API‐style stub (not used by History VM)
        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "")
            => Task.FromResult(new List<WaterQualityRecord>());

        // Satisfy the interface; History VM will call this
        public Task<List<WaterQualityRecord>> GetHistoricalWaterQualityAsync(
            DateTime from, DateTime to, string region)
        {
            return GetWaterQualityAsync(from, to, region);
        }
    }
}
