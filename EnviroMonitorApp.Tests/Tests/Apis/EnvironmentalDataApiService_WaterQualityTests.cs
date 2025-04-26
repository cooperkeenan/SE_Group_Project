using System;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class EnvironmentalDataApiServiceTests
    {
        [Fact]
        public async Task GetWeatherAsync_MapsForecastAndHonorsCache()
        {
            // Arrange: fix “now” as a Unix timestamp
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
                            Temp = 10,
                            Humidity = 50
                        },
                        Wind = new OpenWeatherForecastResponse.WindInfo
                        {
                            Speed = 5
                        }
                    }
                }
            };

            var weatherMock = new Mock<IWeatherApi>(MockBehavior.Strict);
            weatherMock
                .Setup(x => x.GetForecast(
                    51.5074,                 // lat
                    -0.1278,                 // lon
                    It.IsAny<string>(),      // apiKey
                    "metric"                 // units
                ))
                .ReturnsAsync(dto);

            var svc = new EnvironmentalDataApiService(
                airApi:     Mock.Of<IAirQualityApi>(),
                weatherApi: weatherMock.Object,
                waterApi:   Mock.Of<IWaterQualityApi>(),
                keys:       new ApiKeyProvider()
            );

            // Act #1: first call hits the mock
            var first = await svc.GetWeatherAsync();

            // Assert mapping into WeatherRecord
            Assert.Single(first);
            var rec = first[0];
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(fakeDt).DateTime, rec.Timestamp);
            Assert.Equal(10, rec.Temperature);
            Assert.Equal(50, rec.Humidity);
            Assert.Equal(5, rec.WindSpeed);

            // Act #2: immediately again, should use cache
            var second = await svc.GetWeatherAsync();

            // Assert caching: GetForecast only called once total
            weatherMock.Verify(x => x.GetForecast(
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once());
        }
    }
}
