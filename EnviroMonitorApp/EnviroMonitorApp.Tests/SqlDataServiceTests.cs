// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Xunit;
// using EnviroMonitorApp.Models;
// using EnviroMonitorApp.Services;

// namespace EnviroMonitorApp.Tests
// {
//     public class SqlDataServiceTests : IDisposable
//     {
//         private readonly string _tempFolder;
//         private readonly SqlDataService _svc;

//         public SqlDataServiceTests()
//         {
//             _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
//             Directory.CreateDirectory(_tempFolder);

//             var dbPath = Path.Combine(_tempFolder, "enviro.db3");
//             using (var conn = new SQLite.SQLiteConnection(dbPath))
//             {
//                 conn.CreateTable<AirQualityRecord>();
//                 conn.CreateTable<WeatherRecord>();
//                 conn.CreateTable<WaterQualityRecord>();

//                 conn.Execute(@"
//                     CREATE TABLE ClimateRecord (
//                         Date TEXT,
//                         CloudCover REAL,
//                         Sunshine REAL,
//                         GlobalRadiation REAL,
//                         MaxTemp REAL,
//                         MeanTemp REAL,
//                         MinTemp REAL,
//                         Precipitation REAL,
//                         Pressure REAL,
//                         SnowDepth REAL
//                     );");
//             }

//             var fakeFileSystem = new FakeFileSystemService(_tempFolder, dbPath);
//             _svc = new SqlDataService(fakeFileSystem);
//         }

//         // [Fact]
//         // public async Task GetAirQualityAsync_InvalidRange_ReturnsEmpty()
//         // {
//         //     var from = DateTime.UtcNow.AddDays(1);
//         //     var to = DateTime.UtcNow;
//         //     var list = await _svc.GetAirQualityAsync(from, to, "London");
//         //     Assert.Empty(list);
//         // }

//         // [Fact]
//         // public async Task GetWeatherAsync_AlwaysStubbed_ReturnsEmpty()
//         // {
//         //     var list = await _svc.GetWeatherAsync(DateTime.MinValue, DateTime.MaxValue, "Any");
//         //     Assert.Empty(list);
//         // }

//         // [Fact]
//         // public async Task GetWaterQualityAsync_InvalidRange_ReturnsEmpty()
//         // {
//         //     var from = DateTime.UtcNow.AddDays(1);
//         //     var to = DateTime.UtcNow;
//         //     var list = await _svc.GetWaterQualityAsync(from, to, "London");
//         //     Assert.Empty(list);
//         // }

//         // [Fact]
//         // public async Task GetWaterQualityOverload_Hours_EqualsDateRange()
//         // {
//         //     var now = DateTime.UtcNow;
//         //     var earlier = now.AddHours(-2);

//         //     var byHours = await _svc.GetWaterQualityAsync(2, "London");
//         //     var byRange = await _svc.GetWaterQualityAsync(earlier, now, "London");

//         //     Assert.Equal(byHours.Count, byRange.Count);
//         // }

//         // [Fact]
//         // public async Task GetHistoricalWaterQualityAsync_DelegatesToRangeOverload()
//         // {
//         //     var from = DateTime.UtcNow.AddHours(-5);
//         //     var to = DateTime.UtcNow;
//         //     var hist = await _svc.GetHistoricalWaterQualityAsync(from, to, "");
//         //     var direct = await _svc.GetWaterQualityAsync(from, to, "");
//         //     Assert.Equal(direct.Count, hist.Count);
//         // }

//         public void Dispose()
//         {
//             try { Directory.Delete(_tempFolder, recursive: true); } catch { }
//         }

//         private class FakeFileSystemService : IFileSystemService
//         {
//             private readonly string _appDataDir;
//             private readonly string _sourceDbPath;

//             public FakeFileSystemService(string appDataDir, string sourceDbPath)
//             {
//                 _appDataDir = appDataDir;
//                 _sourceDbPath = sourceDbPath;
//             }

//             public string AppDataDirectory => _appDataDir;

//             public Task<Stream> OpenAppPackageFileAsync(string filename)
//             {
//                 Stream stream = File.OpenRead(_sourceDbPath);
//                 return Task.FromResult(stream);
//             }
//         }
//     }
// }
