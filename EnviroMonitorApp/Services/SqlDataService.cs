// EnviroMonitorApp/Services/SqlDataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SQLite;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Opens a local SQLite file, provides CRUD, and can seed from API.
    /// </summary>
    public class SqlDataService : IEnvironmentalDataService
    {
        readonly SQLiteAsyncConnection _db;

        public SqlDataService()
        {
            // Build or open the DB in the app’s data folder
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "enviro.db3");
            _db = new SQLiteAsyncConnection(dbPath);

            // Ensure tables exist (creates file if needed)
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();
        }

        /// <summary>
        /// Delete all old rows and insert fresh.
        /// </summary>
        public async Task SeedAsync(
            List<AirQualityRecord>   air,
            List<WeatherRecord>      weather,
            List<WaterQualityRecord> water)
        {
            await _db.DeleteAllAsync<AirQualityRecord>();
            await _db.InsertAllAsync(air);

            await _db.DeleteAllAsync<WeatherRecord>();
            await _db.InsertAllAsync(weather);

            await _db.DeleteAllAsync<WaterQualityRecord>();
            await _db.InsertAllAsync(water);
        }

        public Task<List<AirQualityRecord>> GetAirQualityAsync(DateTime from, DateTime to, string region) =>
            _db.Table<AirQualityRecord>()
               .Where(r => r.Timestamp >= from && r.Timestamp <= to)
               .OrderBy(r => r.Timestamp)
               .ToListAsync();

        public Task<List<WeatherRecord>> GetWeatherAsync(DateTime from, DateTime to, string region) =>
            _db.Table<WeatherRecord>()
               .Where(r => r.Timestamp >= from && r.Timestamp <= to)
               .OrderBy(r => r.Timestamp)
               .ToListAsync();

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(DateTime from, DateTime to, string region) =>
            _db.Table<WaterQualityRecord>()
               .Where(r => r.Timestamp >= from && r.Timestamp <= to)
               .OrderBy(r => r.Timestamp)
               .ToListAsync();

        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "") =>
            _db.Table<WaterQualityRecord>()
               .Where(r => r.Timestamp >= DateTime.UtcNow.AddHours(-hours))
               .OrderByDescending(r => r.Timestamp)
               .Take(10)
               .ToListAsync();
    }
}
