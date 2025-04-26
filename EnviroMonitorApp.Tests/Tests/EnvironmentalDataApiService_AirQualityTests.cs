using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class EnvironmentalDataApiService_AirQualityTests
    {
        [Fact]
        public async Task GetAirQualityAsync_MapsLocationsAndLatestIntoRecords()
        {
            // Arrange: two fake locations
            var locsDto = new LocationResponse
            {
                Results = new[]
                {
                    new Location { Id = 1, Sensors = new[] {
                        new Sensor { Id = 101, Parameter = new Parameter { Name = "NO2" } },
                        new Sensor { Id = 102, Parameter = new Parameter { Name = "SO2" } }
                    }},
                    new Location { Id = 2, Sensors = new[] {
                        new Sensor { Id = 201, Parameter = new Parameter { Name = "PM25" } },
                        new Sensor { Id = 202, Parameter = new Parameter { Name = "PM10" } }
                    }}
                }
            };

            // And their "latest" readings
            var latest1 = new LocationLatestResponse {
                Results = new[] {
                    new LatestResult { SensorsId = 101, Datetime = new DateWrapper { Utc = "2025-04-25T10:00:00Z" }, Value = 15 },
                    new LatestResult { SensorsId = 102, Datetime = new DateWrapper { Utc = "2025-04-25T10:01:00Z" }, Value = 5  }
                }
            };
            var latest2 = new LocationLatestResponse {
                Results = new[] {
                    new LatestResult { SensorsId = 201, Datetime = new DateWrapper { Utc = "2025-04-25T11:00:00Z" }, Value = 8  },
                    new LatestResult { SensorsId = 202, Datetime = new DateWrapper { Utc = "2025-04-25T11:02:00Z" }, Value = 20 }
                }
            };

            var apiMock = new Mock<IAirQualityApi>(MockBehavior.Strict);
            apiMock
              .Setup(x => x.GetLocations("GB","51.5074,-0.1278",25_000,"7,9,2,1",10))
              .ReturnsAsync(locsDto);
            apiMock.Setup(x => x.GetLocationLatest(1)).ReturnsAsync(latest1);
            apiMock.Setup(x => x.GetLocationLatest(2)).ReturnsAsync(latest2);

            var svc = new EnvironmentalDataApiService(
                airApi: apiMock.Object,
                weatherApi: Mock.Of<IWeatherApi>(),
                waterApi: Mock.Of<IWaterQualityApi>(),
                keys: new ApiKeyProvider()
            );

            // Act
            var list = await svc.GetAirQualityAsync();

            // Assert: two records, with the correct timestamps & values
            Assert.Equal(2, list.Count);

            var r1 = list[0];
            Assert.Equal(DateTime.Parse("2025-04-25T10:01:00Z"), r1.Timestamp);
            Assert.Equal(15, r1.NO2);
            Assert.Equal(5,  r1.SO2);
            Assert.Equal(0,  r1.PM25);
            Assert.Equal(0,  r1.PM10);

            var r2 = list[1];
            Assert.Equal(DateTime.Parse("2025-04-25T11:02:00Z"), r2.Timestamp);
            Assert.Equal(0,  r2.NO2);
            Assert.Equal(0,  r2.SO2);
            Assert.Equal(8,  r2.PM25);
            Assert.Equal(20, r2.PM10);
        }

    }

}