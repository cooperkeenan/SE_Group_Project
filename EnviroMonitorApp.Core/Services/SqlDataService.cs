using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Implementation of IEnvironmentalDataService that uses SQLite database
    /// to store and retrieve environmental data.
    /// </summary>
    public class SqlDataService : IEnvironmentalDataService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly IFileSystemService _fileSystem;
        private const string FileName = "enviro.db3";

        /// <summary>
        /// Initializes a new instance of the SqlDataService class.
        /// Sets up the SQLite database connection and ensures tables are created.
        /// </summary>
        /// <param name="fileSystem">File system service to access app directories and files.</param>
        public SqlDataService(IFileSystemService fileSystem)
        {
            _fileSystem = fileSystem;
            SQLitePCL.Batteries_V2.Init();

            var folder = _fileSystem.AppDataDirectory;
            var dest = Path.Combine(folder, FileName);
            Debug.WriteLine($"[SqlDataService] DB path: {dest}");

            if (!File.Exists(dest))
            {
                Debug.WriteLine($"[SqlDataService] Copying bundled DB → {dest}");
                var copyTask = CopyBundledDbAsync(dest);
                copyTask.Wait();
            }

            _db = new SQLiteAsyncConnection(dest);
            _db.CreateTableAsync<AirQualityRecord>().Wait();
            _db.CreateTableAsync<WeatherRecord>().Wait();
            _db.CreateTableAsync<WaterQualityRecord>().Wait();

            LogTableCounts().Wait();
        }

        /// <summary>
        /// Copies the bundled database file to the application data directory.
        /// </summary>
        /// <param name="dest">Destination path for the copied database file.</param>
        private async Task CopyBundledDbAsync(string dest)
        {
            try
            {
                await using var src = await _fileSystem.OpenAppPackageFileAsync(FileName);
                await using var outp = File.Create(dest);
                await src.CopyToAsync(outp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SqlDataService] Error copying DB: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Logs the number of records in each table to debug output.
        /// </summary>
        private async Task LogTableCounts()
        {
            var totalAir = await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AirQualityRecord");
            var totalWea = await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WeatherRecord");
            var totalWatr = await _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM WaterQualityRecord");

            Debug.WriteLine($"[SqlDataService] AirQuality rows: {totalAir}");
            Debug.WriteLine($"[SqlDataService] WeatherRecord rows: {totalWea}");
            Debug.WriteLine($"[SqlDataService] WaterQuality rows: {totalWatr}");
        }

        /// <summary>
        /// Raw air quality data class for database queries.
        /// </summary>
        private class RawAir
        {
            public string Timestamp { get; set; } = "";
            public double? NO2 { get; set; }
            public double? SO2 { get; set; }
            public double? PM25 { get; set; }
            public double? PM10 { get; set; }
        }

        /// <summary>
        /// Raw climate data class for database queries.
        /// </summary>
        private class RawClimate
        {
            public string Date { get; set; } = "";
            public double? CloudCover { get; set; }
            public double? Sunshine { get; set; }
            public double? GlobalRadiation { get; set; }
            public double? MaxTemp { get; set; }
            public double? MeanTemp { get; set; }
            public double? MinTemp { get; set; }
            public double? Precipitation { get; set; }
            public double? Pressure { get; set; }
            public double? SnowDepth { get; set; }
        }

        /// <summary>
        /// Raw water quality data class for database queries.
        /// </summary>
        private class RawWater
        {
            public string Timestamp { get; set; } = "";
            public double? Nitrate { get; set; }
            public double? PH { get; set; }
            public double? DissolvedOxygen { get; set; }
            public double? Temperature { get; set; }
        }

        /// <summary>
        /// Retrieves air quality data for a specified time period and region from the SQLite database.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of air quality records for the specified period and region.</returns>
        public async Task<List<AirQualityRecord>> GetAirQualityAsync(DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading AirQualityRecord rows");
            var raws = await _db.QueryAsync<RawAir>(
                @"SELECT Timestamp, NO2, SO2, PM25, PM10 FROM AirQualityRecord");

            var toInclusive = to.Date.AddDays(1).AddTicks(-1);
            var list = new List<AirQualityRecord>();

            foreach (var r in raws)
            {
                if (!TryParseTimestamp(r.Timestamp, out var dt)) continue;
                if (dt < from || dt > toInclusive) continue;

                list.Add(new AirQualityRecord
                {
                    Timestamp = dt,
                    NO2 = r.NO2 ?? 0,
                    SO2 = r.SO2 ?? 0,
                    PM25 = r.PM25 ?? 0,
                    PM10 = r.PM10 ?? 0
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed Air rows: {list.Count}");
            return list;
        }

        /// <summary>
        /// Retrieves weather data for a specified time period and region from the SQLite database.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of weather records for the specified period and region.</returns>
        public async Task<List<WeatherRecord>> GetWeatherAsync(DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading ClimateRecord rows");

            var fromDate = from.ToString("yyyy-MM-dd");
            var toDate = to.ToString("yyyy-MM-dd");

            const string sql = @"
                SELECT
                  Date,
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
                ORDER BY Date";

            var raws = await _db.QueryAsync<RawClimate>(sql, fromDate, toDate);
            var list = new List<WeatherRecord>();

            foreach (var r in raws)
            {
                if (!DateTime.TryParseExact(r.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out var dt))
                {
                    Debug.WriteLine($"[SqlDataService] ⚠ Could not parse climate date «{r.Date}»");
                    continue;
                }

                list.Add(new WeatherRecord
                {
                    Timestamp = dt,
                    CloudCover = r.CloudCover ?? 0,
                    Sunshine = r.Sunshine ?? 0,
                    GlobalRadiation = r.GlobalRadiation ?? 0,
                    MaxTemp = r.MaxTemp ?? 0,
                    MeanTemp = r.MeanTemp ?? 0,
                    MinTemp = r.MinTemp ?? 0,
                    Precipitation = r.Precipitation ?? 0,
                    Pressure = r.Pressure ?? 0,
                    SnowDepth = r.SnowDepth ?? 0
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed Weather rows: {list.Count}");
            return list;
        }

        /// <summary>
        /// Retrieves water quality data for a specified time period and region from the SQLite database.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of water quality records for the specified period and region.</returns>
        public async Task<List<WaterQualityRecord>> GetWaterQualityAsync(DateTime from, DateTime to, string region)
        {
            Debug.WriteLine("[SqlDataService] Loading WaterQualityRecord rows");

            var fromDate = from.ToString("yyyy-MM-dd");
            var toDate = to.ToString("yyyy-MM-dd");

            const string sql = @"
                SELECT Timestamp, Nitrate, PH, DissolvedOxygen, Temperature
                FROM WaterQualityRecord
                WHERE Timestamp >= ? AND Timestamp <= ?
                ORDER BY Timestamp";

            var raws = await _db.QueryAsync<RawWater>(sql, fromDate, toDate);
            var list = new List<WaterQualityRecord>();

            foreach (var r in raws)
            {
                if (!TryParseTimestamp(r.Timestamp, out var dt)) continue;

                list.Add(new WaterQualityRecord
                {
                    Timestamp = dt,
                    Nitrate = r.Nitrate,
                    PH = r.PH,
                    DissolvedOxygen = r.DissolvedOxygen,
                    Temperature = r.Temperature
                });
            }

            Debug.WriteLine($"[SqlDataService] Parsed Water rows: {list.Count}");
            return list;
        }

        /// <summary>
        /// Attempts to parse a timestamp string in various formats.
        /// </summary>
        /// <param name="raw">The timestamp string to parse.</param>
        /// <param name="dt">When this method returns, contains the parsed DateTime if successful.</param>
        /// <returns>True if parsing was successful; otherwise, false.</returns>
        private bool TryParseTimestamp(string raw, out DateTime dt)
        {
            return DateTime.TryParseExact(raw, "yyyy-MM-dd'T'HH:mm:ss'Z'",
                       CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt)
                   || DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                       DateTimeStyles.AdjustToUniversal, out dt);
        }

        /// <summary>
        /// Retrieves water quality data for a specified number of hours up to the present.
        /// </summary>
        /// <param name="hours">Number of hours of data to retrieve.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of water quality records for the specified duration.</returns>
        public Task<List<WaterQualityRecord>> GetWaterQualityAsync(int hours, string region = "")
            => Task.FromResult(new List<WaterQualityRecord>());

        /// <summary>
        /// Retrieves historical water quality data for a specified time period and region.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of historical water quality records for the specified period and region.</returns>
        public Task<List<WaterQualityRecord>> GetHistoricalWaterQualityAsync(DateTime from, DateTime to, string region)
            => GetWaterQualityAsync(from, to, region);
    }
}