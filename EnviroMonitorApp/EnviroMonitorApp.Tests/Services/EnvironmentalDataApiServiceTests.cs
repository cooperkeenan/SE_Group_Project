// EnviroMonitorApp.Tests/Services/EnvironmentalDataApiServiceTests.cs
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;         // DTO & record types live here
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Xunit;

namespace EnviroMonitorApp.Tests.Services
{
    public class EnvironmentalDataApiServiceTests
    {
        [Fact]
        public async Task GetWeatherAsync_Returns_Model_From_Api()
        {
            // Arrange: mock the HTTP API
            var mockWeatherApi = new Mock<IWeatherApi>();
            mockWeatherApi
                .Setup(api => api.GetForecast(
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new OpenWeatherForecastResponse
                {
                    List = new List<OpenWeatherForecastResponse.ListItem>
                    {
                        new OpenWeatherForecastResponse.ListItem
                        {
                            Dt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Main = new OpenWeatherForecastResponse.MainInfo
                            {
                                Temp = 20.1,
                                Humidity = 70
                            },
                            Wind = new OpenWeatherForecastResponse.WindInfo
                            {
                                Speed = 5.5
                            }
                        }
                    }
                });

            var mockAirApi   = new Mock<IAirQualityApi>();
            var mockWaterApi = new Mock<IWaterQualityApi>();
            var keys         = new ApiKeyProvider("dummy-aq-key", "dummy-wx-key");

            var svc = new EnvironmentalDataApiService(
                mockAirApi.Object,
                mockWeatherApi.Object,
                mockWaterApi.Object,
                keys
            );

            // Act
            var result = await svc.GetWeatherAsync(
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow,
                "London"
            );

            // Assert
            Assert.Single(result);
            var wr = result[0];
            Assert.Equal(20.1, wr.Temperature);
            Assert.Equal(70, wr.Humidity);
            Assert.Equal(5.5, wr.WindSpeed);
        }
    }
}
