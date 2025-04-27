using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnviroMonitorApp.Models;
using Microsoft.Data.SqlClient;

namespace EnviroMonitorApp.Services
{
    public class SqlDataService : IEnvironmentalDataService
    {
        private readonly string _connectionString;

        public SqlDataService(string connectionString)
            => _connectionString = connectionString;

        private SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public async Task<List<AirQualityRecord>> 
            GetAirQualityAsync(DateTime from, DateTime to, string region)
        {
            const string sql = @"
                SELECT Timestamp, NO2, SO2, PM25, PM10
                  FROM AirQuality
                 WHERE Timestamp BETWEEN @From AND @To
                   AND (@Region = '' OR Region = @Region)
                 ORDER BY Timestamp";

            using var conn = GetOpenConnection();
            var rows = await conn.QueryAsync<AirQualityRecord>(sql, new {
                From   = from,
                To     = to,
                Region = region ?? ""
            });
            return rows.ToList();
        }

        public async Task<List<WeatherRecord>> 
            GetWeatherAsync(DateTime from, DateTime to, string region)
        {
            const string sql = @"
                SELECT Timestamp, Temperature, Humidity, WindSpeed
                  FROM Weather
                 WHERE Timestamp BETWEEN @From AND @To
                   AND (@Region = '' OR Region = @Region)
                 ORDER BY Timestamp";

            using var conn = GetOpenConnection();
            var rows = await conn.QueryAsync<WeatherRecord>(sql, new {
                From   = from,
                To     = to,
                Region = region ?? ""
            });
            return rows.ToList();
        }

        public async Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(DateTime from, DateTime to, string region)
        {
            const string sql = @"
                SELECT Timestamp, Nitrate, PH, DissolvedOxygen, Temperature
                  FROM WaterQuality
                 WHERE Timestamp BETWEEN @From AND @To
                   AND (@Region = '' OR Region = @Region)
                 ORDER BY Timestamp";

            using var conn = GetOpenConnection();
            var rows = await conn.QueryAsync<WaterQualityRecord>(sql, new {
                From   = from,
                To     = to,
                Region = region ?? ""
            });
            return rows.ToList();
        }

        public Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(int hours, string region = "")
        {
            // Delegate to the DateTime-based overload
            var to   = DateTime.UtcNow;
            var from = to.AddHours(-hours);
            return GetWaterQualityAsync(from, to, region);
        }
    }
}
