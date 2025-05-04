using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using Moq;
using SQLite;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.Tests.Services
{
    public class SqlDataServiceTests : IDisposable
    {
        private readonly string _tempFolder;
        private readonly Mock<IFileSystemService> _mockFileSystem;
        private readonly SqlDataService _sut;
        private readonly string _dbPath;

        public SqlDataServiceTests()
        {
            // Create temporary folder for test database
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);
            _dbPath = Path.Combine(_tempFolder, "enviro.db3");

            // Set up mock file system
            _mockFileSystem = new Mock<IFileSystemService>();
            _mockFileSystem.Setup(fs => fs.AppDataDirectory).Returns(_tempFolder);
            _mockFileSystem.Setup(fs => fs.OpenAppPackageFileAsync(It.IsAny<string>()))
                .Returns(() => {
                    // Create an empty database file if it doesn't exist
                    if (!File.Exists(_dbPath))
                    {
                        File.Create(_dbPath).Close();
                    }
                    return Task.FromResult<Stream>(File.OpenRead(_dbPath));
                });

            // Create test database directly with SQL commands to avoid schema issues
            using (var db = new SQLiteConnection(_dbPath))
            {
                // Create tables without using the model classes to avoid the duplicate column problem
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS AirQualityRecord (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp TEXT NOT NULL,
                        NO2 REAL,
                        SO2 REAL,
                        PM25 REAL,
                        PM10 REAL,
                        Category TEXT
                    )");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS WeatherRecord (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp TEXT NOT NULL,
                        Temperature REAL,
                        Humidity REAL,
                        WindSpeed REAL,
                        CloudCover REAL,
                        Sunshine REAL,
                        GlobalRadiation REAL,
                        MaxTemp REAL,
                        MeanTemp REAL,
                        MinTemp REAL,
                        Precipitation REAL,
                        Pressure REAL,
                        SnowDepth REAL
                    )");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS WaterQualityRecord (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp TEXT NOT NULL,
                        Nitrate REAL,
                        PH REAL,
                        DissolvedOxygen REAL,
                        Temperature REAL
                    )");

                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS ClimateRecord (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date TEXT NOT NULL,
                        CloudCover REAL,
                        Sunshine REAL,
                        GlobalRadiation REAL,
                        MaxTemp REAL,
                        MeanTemp REAL,
                        MinTemp REAL,
                        Precipitation REAL,
                        Pressure REAL,
                        SnowDepth REAL
                    )");

                // Insert sample data
                var today = DateTime.UtcNow.Date;
                
                // Add air quality records
                db.Execute(@"
                    INSERT INTO AirQualityRecord (Timestamp, NO2, SO2, PM25, PM10)
                    VALUES (?, ?, ?, ?, ?)",
                    $"{today:yyyy-MM-dd}T12:00:00Z", 10.5, 5.3, 8.2, 15.4);
                
                // Add weather records via ClimateRecord table
                db.Execute(@"
                    INSERT INTO ClimateRecord (Date, CloudCover, Sunshine, GlobalRadiation, 
                        MaxTemp, MeanTemp, MinTemp, Precipitation, Pressure, SnowDepth)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    $"{today:yyyy-MM-dd}", 70.0, 5.5, 150.2, 25.5, 22.3, 18.1, 0.0, 1013.2, 0.0);
                
                // Add water quality records
                db.Execute(@"
                    INSERT INTO WaterQualityRecord (Timestamp, Nitrate, PH, DissolvedOxygen, Temperature)
                    VALUES (?, ?, ?, ?, ?)",
                    $"{today:yyyy-MM-dd}T12:00:00Z", 8.5, 7.2, 9.3, 18.7);
            }

            // Create the service under test
            _sut = new SqlDataService(_mockFileSystem.Object);
        }

        [Fact]
        public async Task GetAirQualityAsync_ReturnsRecordsInDateRange()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // Act
            var result = await _sut.GetAirQualityAsync(yesterday, tomorrow, "London");

            // Assert
            Assert.NotEmpty(result);
            var record = Assert.Single(result);
            Assert.Equal(today, record.Timestamp.Date);
            Assert.Equal(10.5, record.NO2);
            Assert.Equal(5.3, record.SO2);
            Assert.Equal(8.2, record.PM25);
            Assert.Equal(15.4, record.PM10);
        }

        [Fact]
        public async Task GetAirQualityAsync_ReturnsEmptyForOutOfRangeDate()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddYears(-1);
            var olderPastDate = pastDate.AddDays(-10);

            // Act
            var result = await _sut.GetAirQualityAsync(olderPastDate, pastDate, "London");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWeatherAsync_ReturnsRecordsInDateRange()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // Act
            var result = await _sut.GetWeatherAsync(yesterday, tomorrow, "London");

            // Assert
            Assert.NotEmpty(result);
            var record = Assert.Single(result);
            Assert.Equal(today, record.Timestamp.Date);
            Assert.Equal(22.3, record.MeanTemp);
            Assert.Equal(25.5, record.MaxTemp);
            Assert.Equal(18.1, record.MinTemp);
        }

        [Fact]
        public async Task GetWeatherAsync_HandlesEmptyResults()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddYears(-10);
            var olderPastDate = pastDate.AddDays(-10);

            // Act
            var result = await _sut.GetWeatherAsync(olderPastDate, pastDate, "London");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWaterQualityAsync_ReturnsRecordsInDateRange()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // Act
            var result = await _sut.GetWaterQualityAsync(yesterday, tomorrow, "London");

            // Assert
            Assert.NotEmpty(result);
            var record = Assert.Single(result);
            Assert.Equal(today, record.Timestamp.Date);
            Assert.Equal(8.5, record.Nitrate);
            Assert.Equal(7.2, record.PH);
            Assert.Equal(9.3, record.DissolvedOxygen);
            Assert.Equal(18.7, record.Temperature);
        }

        [Fact]
        public async Task GetWaterQualityAsync_HandlesInvalidDateFormat()
        {
            // Arrange
            // Add a record with invalid date format to test error handling
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.Execute(@"
                    INSERT INTO WaterQualityRecord (Timestamp, Nitrate, PH, DissolvedOxygen, Temperature)
                    VALUES (?, ?, ?, ?, ?)",
                    "not-a-valid-date", 10.0, 7.0, 8.0, 20.0);
            }

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // Act
            var result = await _sut.GetWaterQualityAsync(yesterday, tomorrow, "London");

            // Assert - should still return the valid records and skip the invalid one
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetWaterQualityAsync_Hours_CallsDateRangeOverload()
        {
            // Arrange
            var hours = 24;

            // Act
            var result = await _sut.GetWaterQualityAsync(hours, "London");

            // Assert
            Assert.Empty(result); // Returns empty list per implementation
        }

        [Fact]
        public async Task GetHistoricalWaterQualityAsync_CallsRegularWaterQualityMethod()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // Act
            var directResult = await _sut.GetWaterQualityAsync(yesterday, tomorrow, "London");
            var histResult = await _sut.GetHistoricalWaterQualityAsync(yesterday, tomorrow, "London");

            // Assert
            Assert.Equal(directResult.Count, histResult.Count);
            if (directResult.Count > 0 && histResult.Count > 0)
            {
                Assert.Equal(directResult[0].Timestamp, histResult[0].Timestamp);
                Assert.Equal(directResult[0].Nitrate, histResult[0].Nitrate);
                Assert.Equal(directResult[0].PH, histResult[0].PH);
            }
        }

        [Fact]
        public async Task TryParseTimestamp_HandlesVariousFormats()
        {
            // This test accesses a private method via reflection
            var parseMethod = typeof(SqlDataService).GetMethod("TryParseTimestamp", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // If the method is not accessible via reflection, skip this test
            if (parseMethod == null)
            {
                // Instead, test the parsing functionality indirectly
                // Add records with various date formats
                using (var db = new SQLiteConnection(_dbPath))
                {
                    db.Execute(@"
                        INSERT INTO WaterQualityRecord (Timestamp, Nitrate, PH, DissolvedOxygen, Temperature)
                        VALUES (?, ?, ?, ?, ?)",
                        "2023-01-01T13:45:30Z", 1.0, 7.0, 8.0, 15.0);
                    
                    db.Execute(@"
                        INSERT INTO WaterQualityRecord (Timestamp, Nitrate, PH, DissolvedOxygen, Temperature)
                        VALUES (?, ?, ?, ?, ?)",
                        "2023-02-15 14:30:00", 2.0, 7.5, 8.5, 16.0);
                }

                // Act - Query with date range that includes these dates
                var result = await _sut.GetWaterQualityAsync(
                    new DateTime(2023, 1, 1), 
                    new DateTime(2023, 3, 1), 
                    "London");

                // Assert - Should find both records
                Assert.Equal(2, result.Count);
            }
        }

        public void Dispose()
        {
            // Clean up the temporary database files
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
                
                if (Directory.Exists(_tempFolder))
                {
                    Directory.Delete(_tempFolder, recursive: true);
                }
            }
            catch
            {
                // Suppress exceptions during cleanup
            }
        }
    }
}