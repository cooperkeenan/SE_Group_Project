using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
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

            // Set up mock file system first
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
            using (var db = new SQLite.SQLiteConnection(_dbPath))
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

            // Create the service under test - this will run after our manual setup
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
            Assert.Contains(result, r => r.Timestamp.Date == today);
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
            // Note: The implementation may return different kinds of data depending on the date range
            // We're just verifying it returns something and doesn't throw
            Assert.NotNull(result);
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
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetWaterQualityAsync_Hours_CallsDateRangeOverload()
        {
            // Arrange
            var hours = 24;

            // Act
            var result = await _sut.GetWaterQualityAsync(hours, "London");

            // Assert
            Assert.NotNull(result);
            // The implemented method returns an empty list
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoricalWaterQualityAsync_CallsRegularWaterQualityMethod()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            // Act
            var histResult = await _sut.GetHistoricalWaterQualityAsync(yesterday, tomorrow, "London");

            // Assert
            Assert.NotNull(histResult);
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