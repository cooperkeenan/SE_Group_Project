// EnviroMonitorApp/Services/SqlDataService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            // Copy bundled DB (with pre‐populated tables) into AppData
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

            // Open the connection
            _db = new SQLiteAsyncConnection(dest);
            Debug.WriteLine($"[SqlDataService] Opened SQLite DB");

            // Ensure our three record tables exist
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();

            // Log counts
            var totalAir   = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AirQualityRecord").Result;
            var totalWea   = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WeatherRecord").Result;
            var totalWatr  = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WaterQualityRecord").Result;
            Debug.WriteLine($"[SqlDataService] AirQuality rows: {totalAir}");
            Debug.WriteLine($"[SqlDataService] WeatherRecord rows: {totalWea}");
            Debug.WriteLine($"[SqlDataService] WaterQuality rows: {totalWatr}");
        }

        // ──────────── Raw DTOs for queries ────────────

        private class RawAir
        {
            public string Timestamp { get; set; } = "";
            public double? NO2     { get; set; }
            public double? SO2     { get; set; }
            public double? PM25    { get; set; }
            public double? PM10    { get; set; }
        }

        private class RawClimate
        {
            public string Date              { get; set; } = "";
            public double? CloudCover       { get; set; }
            public double? Sunshine         { get; set; }
            public double? GlobalRadiation  { get; set; }
            public double? MaxTemp          { get; set; }
            public double? MeanTemp         { get; set; }
            public double? MinTemp          { get; set; }
            public double? Precipitation    { get; set; }
            public double? Pressure         { get; set; }
            public double? SnowDepth        { get; set; }
        }

        private class RawWater
        {
            public string Timestamp         { get; set; } = "";
            public double? Nitrate          { get; set; }
            public double? PH               { get; set; }
            public double? DissolvedOxygen  { get; set; }
            public double? Temperature      { get; set; }
        }

        // ──────────── AIR ─────────────────────────

        public async Task<List<AirQualityRecord>> GetAirQualityAsync(
            DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading AirQualityRecord rows");
            var raws = await _db.QueryAsync<RawAir>(
                @"SELECT Timestamp, NO2, SO2, PM25, PM10 FROM AirQualityRecord");

            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            var list = new List<AirQualityRecord>();

            foreach (var r in raws)
            {
                if (!DateTime.TryParseExact(
                        r.Timestamp,
                        "yyyy-MM-dd'T'HH:mm:ss'Z'",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out var dt)
                    && !DateTime.TryParse(
                        r.Timestamp,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out dt))
                {
                    Debug.WriteLine($"[SqlDataService] ⚠ Could not parse air ts «{r.Timestamp}»");
                    continue;
                }

                if (dt < from || dt > toInclusive)
                    continue;

                list.Add(new AirQualityRecord
                {
                    Timestamp = dt,
                    NO2       = r.NO2  ?? 0,
                    SO2       = r.SO2  ?? 0,
                    PM25      = r.PM25 ?? 0,
                    PM10      = r.PM10 ?? 0
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed+filtered Air rows: {list.Count}");
            return list;
        }

        // ──────────── WEATHER (historical CSV) ─────────────────────────

        public async Task<List<WeatherRecord>> GetWeatherAsync(
            DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading ClimateRecord rows for weather");

            // we'll compare only the date portion (YYYY-MM-DD)
            var fromDate = from.ToString("yyyy-MM-dd");
            var toDate   = to  .ToString("yyyy-MM-dd");

            const string sql = @"
                SELECT
                  Date AS Date,
                  CloudCover,
                  Sunshine,
                  GlobalRadiation,
                  MaxTemp,
                  MeanTemp,
                  MinTemp,
                  Precipitation,
                  Pressure,
                  SnowDepth
                FROM ClimateRecord
                WHERE Date >= ? AND Date <= ?
                ORDER BY Date
            ";

            var raws = await _db.QueryAsync<RawClimate>(sql, fromDate, toDate);
            var list = new List<WeatherRecord>();

            foreach (var r in raws)
            {
                if (!DateTime.TryParseExact(
                        r.Date,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var dt))
                {
                    Debug.WriteLine($"[SqlDataService] ⚠ Could not parse climate date «{r.Date}»");
                    continue;
                }

                list.Add(new WeatherRecord
                {
                    Timestamp       = dt,
                    CloudCover      = r.CloudCover       ?? 0,
                    Sunshine        = r.Sunshine         ?? 0,
                    GlobalRadiation = r.GlobalRadiation  ?? 0,
                    MaxTemp         = r.MaxTemp          ?? 0,
                    MeanTemp        = r.MeanTemp         ?? 0,
                    MinTemp         = r.MinTemp          ?? 0,
                    Precipitation   = r.Precipitation    ?? 0,
                    Pressure        = r.Pressure         ?? 0,
                    SnowDepth       = r.SnowDepth        ?? 0
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed+filtered Weather rows: {list.Count}");
            return list;
        }

        // ──────────── WATER ─────────────────────────

        public async Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading WaterQualityRecord rows");

            var fromDate = from.ToString("yyyy-MM-dd");
            var toDate   = to  .ToString("yyyy-MM-dd");

            const string sql = @"
                SELECT Timestamp, Nitrate, PH, DissolvedOxygen, Temperature
                  FROM WaterQualityRecord
                 WHERE Timestamp >= ? AND Timestamp <= ?
                 ORDER BY Timestamp
            ";

            var raws = await _db.QueryAsync<RawWater>(sql, fromDate, toDate);
            var list = new List<WaterQualityRecord>();

            foreach (var r in raws)
            {
                if (!DateTime.TryParseExact(
                        r.Timestamp,
                        "yyyy-MM-dd'T'HH:mm:ss'Z'",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out var dt))
                {
                    Debug.WriteLine($"[SqlDataService] ⚠ Could not parse water ts «{r.Timestamp}»");
                    continue;
                }

                list.Add(new WaterQualityRecord
                {
                    Timestamp       = dt,
                    Nitrate         = r.Nitrate,
                    PH              = r.PH,
                    DissolvedOxygen = r.DissolvedOxygen,
                    Temperature     = r.Temperature
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed+filtered Water rows: {list.Count}");
            return list;
        }

        // optional API‐style overload (unused in History VM)
        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "")
            => Task.FromResult(new List<WaterQualityRecord>());

        // for the interface
        public Task<List<WaterQualityRecord>> GetHistoricalWaterQualityAsync(
            DateTime from, DateTime to, string region)
            => GetWaterQualityAsync(from, to, region);
    }
}
