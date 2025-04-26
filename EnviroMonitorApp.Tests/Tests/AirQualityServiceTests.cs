using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class AirQualityServiceTests
    {
        [Fact]
        public async Task GetAirQualityAsync_ReturnsOneRecordPerLocation_WithCorrectValues()
        {
            // Arrange
            var locs = new LocationResponse
            {
                Results = new[]
                {
                    new Location { Id = 1, Sensors = new[]
                        {
                            new Sensor { Id = 11, Parameter = new Parameter { Name = "no2" } },
                            new Sensor { Id = 12, Parameter = new Parameter { Name = "so2" } },
                            new Sensor { Id = 13, Parameter = new Parameter { Name = "pm25" } },
                            new Sensor { Id = 14, Parameter = new Parameter { Name = "pm10" } },
                        }
                    },
                    new Location { Id = 2, Sensors = new[]
                        {
                            new Sensor { Id = 21, Parameter = new Parameter { Name = "no2" } },
                            new Sensor { Id = 22, Parameter = new Parameter { Name = "so2" } },
                            new Sensor { Id = 23, Parameter = new Parameter { Name = "pm25" } },
                            new Sensor { Id = 24, Parameter = new Parameter { Name = "pm10" } },
                        }
                    }
                }
            };

            // For each locationId, return LatestResponse with one reading per sensor
            var airApi = new Mock<IAirQualityApi>(MockBehavior.Strict);
            airApi
                .Setup(a => a.GetLocations("GB", "51.5074,-0.1278", 25000, "7,9,2,1", 10))
                .ReturnsAsync(locs);

            LocationLatestResponse MakeLatest(int locationId)
            {
                // assign different timestamps per pollutant per location
                var baseTime = DateTime.UtcNow.Date.AddHours(locationId);
                return new LocationLatestResponse
                {
                    Results = new[]
                    {
                        new LatestResult { SensorsId = locationId * 10 + 1, Datetime = new DateWrapper { Utc = baseTime.AddMinutes(1).ToString("o") }, Value = 1.1 * locationId },
                        new LatestResult { SensorsId = locationId * 10 + 2, Datetime = new DateWrapper { Utc = baseTime.AddMinutes(2).ToString("o") }, Value = 2.2 * locationId },
                        new LatestResult { SensorsId = locationId * 10 + 3, Datetime = new DateWrapper { Utc = baseTime.AddMinutes(3).ToString("o") }, Value = 3.3 * locationId },
                        new LatestResult { SensorsId = locationId * 10 + 4, Datetime = new DateWrapper { Utc = baseTime.AddMinutes(4).ToString("o") }, Value = 4.4 * locationId },
                    }
                };
            }

            airApi
                .Setup(a => a.GetLocationLatest(1))
                .ReturnsAsync(MakeLatest(1));
            airApi
                .Setup(a => a.GetLocationLatest(2))
                .ReturnsAsync(MakeLatest(2));

            // The other dependencies can be no-ops
            var wxApi = Mock.Of<IWeatherApi>();
            var wqApi = Mock.Of<IWaterQualityApi>();
            var keys  = new ApiKeyProvider();

            var svc = new EnvironmentalDataApiService(airApi.Object, wxApi, wqApi, keys);

            // Act
            var records = await svc.GetAirQualityAsync();

            // Assert: one record per location
            Assert.Equal(2, records.Count);

            // Check first location
            var r1 = records.First(r => r.NO2  == 1.1 * 1);
            Assert.Equal(1.1 * 1, r1.NO2, 3);
            Assert.Equal(2.2 * 1, r1.SO2, 3);
            Assert.Equal(3.3 * 1, r1.PM25,3);
            Assert.Equal(4.4 * 1, r1.PM10,3);
            // timestamp should be the max of the four
            Assert.Equal(DateTime.Parse(MakeLatest(1).Results.Max(m => m.Datetime.Utc)), r1.Timestamp);

            // Check second location
            var r2 = records.First(r => r.NO2  == 1.1 * 2);
            Assert.Equal(1.1 * 2, r2.NO2, 3);
            Assert.Equal(2.2 * 2, r2.SO2, 3);
            Assert.Equal(3.3 * 2, r2.PM25,3);
            Assert.Equal(4.4 * 2, r2.PM10,3);
            Assert.Equal(DateTime.Parse(MakeLatest(2).Results.Max(m => m.Datetime.Utc)), r2.Timestamp);

            airApi.VerifyAll();
        }
    }
}
