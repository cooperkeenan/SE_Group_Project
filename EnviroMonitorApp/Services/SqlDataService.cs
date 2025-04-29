// EnviroMonitorApp/Services/SqlDataService.cs
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
        readonly EnvironmentalDataApiService _api;
        const string FileName = "enviro.db3";
        const int ChunkSizeDays = 1; // one‐day chunks

        public SqlDataService(EnvironmentalDataApiService apiService)
        {
            _api = apiService;

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

        // This is your public SeedAsync that AppShell is calling
        public Task SeedAsync(DateTime from, DateTime to, string region)
            => SeedRangeAsync(from, to, region);

        // internal workhorse: fetch day‐by‐day
        async Task SeedRangeAsync(DateTime from, DateTime to, string region)
        {
            if (region == "All") region = "London";

            var cursor = from.Date;
            var end    = to.Date;
            while (cursor <= end)
            {
                var start = cursor;
                var finish = cursor.AddDays(1).AddTicks(-1);
                Debug.WriteLine($"[SqlDataService] Fetching chunk {start:MM/dd}→{finish:MM/dd}");
                await FetchAndInsertChunkAsync(start, finish, region);

                cursor = cursor.AddDays(ChunkSizeDays);
                await Task.Delay(200);  // small throttle
            }
        }

        async Task FetchAndInsertChunkAsync(DateTime chunkStart, DateTime chunkEnd, string region)
        {
            try
            {
                var list = await _api.GetAirQualityAsync(chunkStart, chunkEnd, region);
                if (list?.Count > 0)
                {
                    await _db.InsertAllAsync(list);
                    Debug.WriteLine($"[SqlDataService] Chunk {chunkStart:MM/dd} OK");
                }
            }
            catch (Refit.ApiException ae) when ((int)ae.StatusCode == 429)
            {
                Debug.WriteLine($"[SqlDataService] 429 on {chunkStart:MM/dd}, retrying");
                await Task.Delay(1000);
                await FetchAndInsertChunkAsync(chunkStart, chunkEnd, region);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SqlDataService] ⚠️ chunk {chunkStart:MM/dd} failed: {ex.GetType().Name}");
            }
        }

        public async Task<List<AirQualityRecord>> GetAirQualityAsync(
            DateTime from, DateTime to, string region)
        {
            // make sure DB is seeded for that entire span
            await SeedRangeAsync(from, to, region);

            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            return await _db.Table<AirQualityRecord>()
                            .Where(r => r.Timestamp >= from && r.Timestamp <= toInclusive)
                            .OrderBy(r => r.Timestamp)
                            .ToListAsync();
        }

        public Task<List<WeatherRecord>> GetWeatherAsync(DateTime from, DateTime to, string region)
        {
            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            return _db.Table<WeatherRecord>()
                      .Where(r => r.Timestamp >= from && r.Timestamp <= toInclusive)
                      .OrderBy(r => r.Timestamp)
                      .ToListAsync();
        }

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(DateTime from, DateTime to, string region)
        {
            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            return _db.Table<WaterQualityRecord>()
                      .Where(r => r.Timestamp >= from && r.Timestamp <= toInclusive)
                      .OrderBy(r => r.Timestamp)
                      .ToListAsync();
        }

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "")
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
