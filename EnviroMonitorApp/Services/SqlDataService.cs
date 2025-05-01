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
            // 1) Copy your pre-populated DB into AppData (always overwrite so you pick
            //    up new exports from your repo).
            var folder = FileSystem.AppDataDirectory;
            var dest   = Path.Combine(folder, FileName);
            Debug.WriteLine($"[SqlDataService] Dest DB path: {dest}");

            if (File.Exists(dest))
            {
                Debug.WriteLine($"[SqlDataService] Deleting existing DB");
                File.Delete(dest);
            }

            Debug.WriteLine($"[SqlDataService] Copying bundle DB → {dest}");
            // NOTE: FileSystem.OpenAppPackageFileAsync will look in Resources/Raw
            using var src  = FileSystem.OpenAppPackageFileAsync(FileName).Result;
            using var outp = File.Create(dest);
            src.CopyTo(outp);

            // 2) Open SQLite
            _db = new SQLiteAsyncConnection(dest);
            Debug.WriteLine($"[SqlDataService] Opened SQLite DB");

            // 3) Ensure tables exist (no-ops if schema matches)
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();

            // 4) Log how many rows shipped
            var total = _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AirQualityRecord").Result;
            Debug.WriteLine($"[SqlDataService] Total rows in AirQualityRecord: {total}");
        }

        // intermediate type for raw SQL
        class RawRecord
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
            Debug.WriteLine($"[SqlDataService] Loading all raw rows");
            // grab the TEXT timestamp + values
            var raws = await _db.QueryAsync<RawRecord>(
                "SELECT Timestamp, NO2, SO2, PM25, PM10 FROM AirQualityRecord");

            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            var outList = new List<AirQualityRecord>(raws.Count);

            foreach (var r in raws)
            {
                // try our strict ISO-8601 parse first
                if (!DateTime.TryParseExact(
                        r.Timestamp,
                        "yyyy-MM-dd'T'HH:mm:ss'Z'",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var dt)
                    // fallback to a more forgiving parse if needed
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

            Debug.WriteLine($"[SqlDataService] Parsed+filtered rows: {outList.Count}");
            return outList;
        }

        // stub out the others to satisfy the interface
        public Task<List<WeatherRecord>> GetWeatherAsync(DateTime from, DateTime to, string region)
            => Task.FromResult(new List<WeatherRecord>());
        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(DateTime from, DateTime to, string region)
            => Task.FromResult(new List<WaterQualityRecord>());
        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "")
            => Task.FromResult(new List<WaterQualityRecord>());
    }
}
