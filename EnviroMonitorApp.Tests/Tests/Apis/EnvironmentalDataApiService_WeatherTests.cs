using System;
using System.Reflection;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class EnvironmentalDataApiService_WeatherTests
    {
        public EnvironmentalDataApiService_WeatherTests()
        {
            // Reset the static weather cache before each test
            var svcType = typeof(EnvironmentalDataApiService);
            svcType.GetField("_wxCache", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, null);
            svcType.GetField("_wxStamp", BindingFlags.Static | BindingFlags.NonPublic)
                   .SetValue(null, DateTime.MinValue);
        }

        [Fact]
        public async Task GetWeatherAsync_MapsForecastIntoRecords()
        {
            // Arrange: use a fixed Unix timestamp
            var fakeDt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var dto = new OpenWeatherForecastResponse
            {
                List = new[]
                {
                    new OpenWeatherForecastResponse.ListItem
                    {
                        Dt = fakeDt,
                        Main = new OpenWeatherForecastResponse.MainInfo
                        {
                            Temp = 22.5,
                            Humidity = 60
                        },
                        Wind = new OpenWeatherForecastResponse.WindInfo
                        {
                            Speed = 7
                        }
                    }
                }
            };

            var weatherMock = new Mock<IWeatherApi>(MockBehavior.Strict);
            weatherMock
                .Setup(x => x.GetForecast(
                    51.5074,
                    -0.1278,
                    It.IsAny<string>(),
                    "metric"
                ))
                .ReturnsAsync(dto);

            var svc = new EnvironmentalDataApiService(
                airApi:    Mock.Of<IAirQualityApi>(),
                weatherApi: weatherMock.Object,
                waterApi:  Mock.Of<IWaterQualityApi>(),
                keys:      new ApiKeyProvider()
            );

            // Act
            var result = await svc.GetWeatherAsync();

            // Assert mapping into WeatherRecord
            Assert.Single(result);
            var rec = result[0];
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(fakeDt).DateTime, rec.Timestamp);
            Assert.Equal(22.5, rec.Temperature);
            Assert.Equal(60,   rec.Humidity);
            Assert.Equal(7,    rec.WindSpeed);
        }

        [Fact]
        public async Task GetWeatherAsync_UsesCacheWithin10Minutes()
        {
            // Arrange: dummy forecast
            var dto = new OpenWeatherForecastResponse
            {
                List = new[] { new OpenWeatherForecastResponse.ListItem { Dt = 0, Main = new OpenWeatherForecastResponse.MainInfo(), Wind = new OpenWeatherForecastResponse.WindInfo() } }
            };

            var weatherMock = new Mock<IWeatherApi>(MockBehavior.Strict);
            weatherMock
                .Setup(x => x.GetForecast(
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(dto);

            var svc = new EnvironmentalDataApiService(
                airApi:    Mock.Of<IAirQualityApi>(),
                weatherApi: weatherMock.Object,
                waterApi:  Mock.Of<IWaterQualityApi>(),
                keys:      new ApiKeyProvider()
            );

            // Act #1
            await svc.GetWeatherAsync();
            // Act #2 immediately
            await svc.GetWeatherAsync();

            // Assert underlying API called only once
            weatherMock.Verify(x => x.GetForecast(
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once());
        }
    }
}