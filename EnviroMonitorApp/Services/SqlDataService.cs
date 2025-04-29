// Services/SqlDataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using EnviroMonitorApp.Models;
using System.Diagnostics;

namespace EnviroMonitorApp.Services
{
    public class SqlDataService : IEnvironmentalDataService
    {
        const string FileName = "enviro.db3";
        readonly SQLiteAsyncConnection _db;
        readonly EnvironmentalDataApiService _api;

        public SqlDataService(EnvironmentalDataApiService apiService)
        {
            _api = apiService;

            // Copy blank DB into AppData if needed
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

            // open & ensure tables
            _db = new SQLiteAsyncConnection(path);
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();
        }

        /// <summary>
        /// Seed any missing days in [from,to] by pulling each day individually.
        /// </summary>
        async Task EnsureSeededFor(DateTime from, DateTime to, string region)
        {
            // inclusive-to-the-end-of-day
            var toInclusive = to.Date.AddDays(1).AddTicks(-1);

            // normalize “All” → “London”
            if (region == "All") region = "London";

            // fetch existing earliest & latest
            var first = await _db.Table<AirQualityRecord>()
                                 .OrderBy(r => r.Timestamp)
                                 .FirstOrDefaultAsync();
            var last  = await _db.Table<AirQualityRecord>()
                                 .OrderByDescending(r => r.Timestamp)
                                 .FirstOrDefaultAsync();

            // if no data, seed every day in the span
            if (first == null || last == null)
            {
                Debug.WriteLine($"[SqlDataService] No data; seeding every day {from:MM/dd}→{toInclusive:MM/dd}");
                await FetchAndUpsertRange(from.Date, toInclusive, region);
                return;
            }

            // fetch days before what you have
            if (from.Date < first.Timestamp.Date)
            {
                var start = from.Date;
                var end   = first.Timestamp.Date.AddTicks(-1);
                Debug.WriteLine($"[SqlDataService] Seeding BEFORE {first.Timestamp:MM/dd}: {start:MM/dd}→{end:MM/dd}");
                await FetchAndUpsertRange(start, end, region);
            }

            // fetch days after what you have
            if (toInclusive.Date > last.Timestamp.Date)
            {
                var start = last.Timestamp.Date.AddDays(1);
                var end   = toInclusive;
                Debug.WriteLine($"[SqlDataService] Seeding AFTER  {last.Timestamp:MM/dd}: {start:MM/dd}→{end:MM/dd}");
                await FetchAndUpsertRange(start, end, region);
            }
        }

        /// <summary>
        /// Break [dayStart → dayEnd] into per‐calendar‐day sub‐ranges, fetch & upsert them.
        /// </summary>
        async Task FetchAndUpsertRange(DateTime dayStart, DateTime dayEnd, string region)
        {
            var cursor = dayStart.Date;
            var endDay = dayEnd.Date;
            while (cursor <= endDay)
            {
                var chunkFrom = cursor;
                var chunkTo   = cursor.AddDays(1).AddTicks(-1);
                try
                {
                    Debug.WriteLine($"[SqlDataService]  Fetching chunk {chunkFrom:MM/dd}→{chunkTo:MM/dd}");
                    var list = await _api.GetAirQualityAsync(chunkFrom, chunkTo, region);
                    if (list?.Count > 0)
                        await _db.RunInTransactionAsync(conn =>
                        {
                            foreach (var r in list)
                                conn.InsertOrReplace(r);
                        });
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    Debug.WriteLine($"[SqlDataService]  ⚠️ chunk {chunkFrom:MM/dd} failed: {ex.Message}");
                    // swallow so a bad single day doesn't kill the whole seed.
                }

                cursor = cursor.AddDays(1);
            }
        }

        /// <summary>
        /// Public seeding entry point (called by AppShell once).
        /// </summary>
        public async Task SeedAsync(DateTime from, DateTime to, string region)
        {
            try
            {
                await EnsureSeededFor(from, to, region);
                Debug.WriteLine($"[SqlDataService] SeedAsync complete for {from:MM/dd}→{to:MM/dd}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SqlDataService] SeedAsync FAILED: {ex}");
                throw;
            }
        }

        public async Task<List<AirQualityRecord>> GetAirQualityAsync(
            DateTime from, DateTime to, string region)
        {
            await EnsureSeededFor(from, to, region);
            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            return await _db.Table<AirQualityRecord>()
                            .Where(r => r.Timestamp >= from 
                                     && r.Timestamp <= toInclusive)
                            .OrderBy(r => r.Timestamp)
                            .ToListAsync();
        }

        public Task<List<WeatherRecord>> GetWeatherAsync(
            DateTime from, DateTime to, string region)
        {
            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            return _db.Table<WeatherRecord>()
                      .Where(r => r.Timestamp >= from 
                               && r.Timestamp <= toInclusive)
                      .OrderBy(r => r.Timestamp)
                      .ToListAsync();
        }

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            DateTime from, DateTime to, string region)
        {
            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            return _db.Table<WaterQualityRecord>()
                      .Where(r => r.Timestamp >= from 
                               && r.Timestamp <= toInclusive)
                      .OrderBy(r => r.Timestamp)
                      .ToListAsync();
        }

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(
            int hours, string region = "")
        {
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            return _db.Table<WaterQualityRecord>()
                      .Where(r => r.Timestamp >= cutoff)
                      .OrderByDescending(r => r.Timestamp)
                      .Take(10)
                      .ToListAsync();
        }
    }
}
