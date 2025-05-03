using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Tests
{
    public class SqlDataServiceTests : IDisposable
    {
        readonly string _tempFolder;
        readonly SqlDataService _svc;

        public SqlDataServiceTests()
        {
            // 1. Create a temp directory to simulate AppDataDirectory
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);

            // 2. Create an empty test DB
            var dest = Path.Combine(_tempFolder, "enviro.db3");
            using (var conn = new SQLite.SQLiteConnection(dest))
            {
                conn.CreateTable<AirQualityRecord>();
                conn.CreateTable<WeatherRecord>();
                conn.CreateTable<WaterQualityRecord>();
            }

            // 3. Use the new override parameter to inject the test path
            _svc = new SqlDataService(dbPathOverride: _tempFolder);
        }

        [Fact]
        public async Task GetAirQualityAsync_InvalidRange_ReturnsEmpty()
        {
            var from = DateTime.UtcNow.AddDays(1);
            var to   = DateTime.UtcNow;
            var list = await _svc.GetAirQualityAsync(from, to, "London");
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetWeatherAsync_AlwaysStubbed_ReturnsEmpty()
        {
            var list = await _svc.GetWeatherAsync(DateTime.MinValue, DateTime.MaxValue, "Any");
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetWaterQualityAsync_InvalidRange_ReturnsEmpty()
        {
            var from = DateTime.UtcNow.AddDays(1);
            var to   = DateTime.UtcNow;
            var list = await _svc.GetWaterQualityAsync(from, to, "London");
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetWaterQualityOverload_Hours_EqualsDateRange()
        {
            var now     = DateTime.UtcNow;
            var earlier = now.AddHours(-2);

            var byHours = await _svc.GetWaterQualityAsync(2, "London");
            var byRange = await _svc.GetWaterQualityAsync(earlier, now, "London");

            Assert.Equal(byHours.Count, byRange.Count);
        }

        [Fact]
        public async Task GetHistoricalWaterQualityAsync_DelegatesToRangeOverload()
        {
            var from = DateTime.UtcNow.AddHours(-5);
            var to   = DateTime.UtcNow;
            var hist = await _svc.GetHistoricalWaterQualityAsync(from, to, "");
            var direct = await _svc.GetWaterQualityAsync(from, to, "");
            Assert.Equal(direct.Count, hist.Count);
        }

        public void Dispose()
        {
            try { Directory.Delete(_tempFolder, recursive: true); }
            catch { /* ignore */ }
        }
    }
}
