// Services/SqlDataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using System.Diagnostics;

namespace EnviroMonitorApp.Services
{
    public class SqlDataService : IEnvironmentalDataService
    {
        readonly SQLiteAsyncConnection _db;
        readonly EnvironmentalDataApiService _api;  // concrete API service
        const string FileName = "enviro.db3";

        public SqlDataService(EnvironmentalDataApiService apiService)
        {
            _api = apiService;

            // Copy the blank database from Data/enviro.db3 into AppData
            var folder = FileSystem.AppDataDirectory;
            var path   = Path.Combine(folder, FileName);

            if (!File.Exists(path))
            {
                Debug.WriteLine($"[SqlDataService] Copying blank DB to {path}");
                using var src = FileSystem.OpenAppPackageFileAsync(Path.Combine("Data", FileName)).Result;
                using var dst = File.Create(path);
                src.CopyTo(dst);
            }
            else
            {
                Debug.WriteLine($"[SqlDataService] Using existing DB at {path}");
            }

            _db = new SQLiteAsyncConnection(path);
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();
        }

        /// <summary>
        /// Ensures any missing days in [from,to] are fetched from the API
        /// and inserted into SQLite before we read.
        /// </summary>
        async Task EnsureSeededFor(DateTime from, DateTime to, string region)
        {
            var first = await _db.Table<AirQualityRecord>()
                                 .OrderBy(r => r.Timestamp)
                                 .FirstOrDefaultAsync();

            if (first == null)
            {
                Debug.WriteLine($"[SqlDataService] No data; seeding entire {from:MM/dd}→{to:MM/dd}");
                var all = await _api.GetAirQualityAsync(from, to, region);
                await _db.InsertAllAsync(all);
                return;
            }

            if (from < first.Timestamp)
            {
                Debug.WriteLine($"[SqlDataService] Fetching missing {from:MM/dd}→{first.Timestamp:MM/dd}");
                var missing = await _api.GetAirQualityAsync(from, first.Timestamp, region);
                if (missing?.Count > 0)
                    await _db.InsertAllAsync(missing);
            }
        }

        public async Task<List<AirQualityRecord>> GetAirQualityAsync(
            DateTime from, DateTime to, string region)
        {
            // 1) On-demand seed
            await EnsureSeededFor(from, to, region);

            // 2) Then just return the slice
            return await _db.Table<AirQualityRecord>()
                            .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                            .OrderBy(r => r.Timestamp)
                            .ToListAsync();
        }

        public Task<List<WeatherRecord>> GetWeatherAsync(
            DateTime from, DateTime to, string region)
            => _db.Table<WeatherRecord>()
                  .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                  .OrderBy(r => r.Timestamp)
                  .ToListAsync();

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            DateTime from, DateTime to, string region)
            => _db.Table<WaterQualityRecord>()
                  .Where(r => r.Timestamp >= from && r.Timestamp <= to)
                  .OrderBy(r => r.Timestamp)
                  .ToListAsync();

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            int hours, string region = "")
            => _db.Table<WaterQualityRecord>()
                  .Where(r => r.Timestamp >= DateTime.UtcNow.AddHours(-hours))
                  .OrderByDescending(r => r.Timestamp)
                  .Take(10)
                  .ToListAsync();
    }
}
