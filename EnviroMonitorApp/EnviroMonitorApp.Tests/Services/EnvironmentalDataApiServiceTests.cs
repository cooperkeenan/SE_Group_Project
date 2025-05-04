using Moq;
using System;
using System.Threading.Tasks;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services.Apis; 
using Xunit;

namespace EnviroMonitorApp.Tests.Services
{
    public class EnvironmentalDataApiServiceTests
    {
        [Fact]
        public async Task GetWeatherAsync_Returns_Model_From_Api()
        {
            // Arrange: Use the actual ApiKeyProvider constructor with keys
            var apiKeyProvider = new ApiKeyProvider("mock_open_aq_key", "mock_open_weather_map_key");

            // Mock the other dependencies
            var mockAirQualityApi = new Mock<IAirQualityApi>();
            var mockWeatherApi = new Mock<IWeatherApi>();
            mockWeatherApi.Setup(a => a.GetWeatherAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                        .ReturnsAsync(new WeatherRecord { Temperature = 20.1, Humidity = 70 });

            var mockWaterQualityApi = new Mock<IWaterQualityApi>();

            // Instantiate the service with the real ApiKeyProvider
            var service = new EnvironmentalDataApiService(
                mockAirQualityApi.Object,
                mockWeatherApi.Object,
                mockWaterQualityApi.Object,
                apiKeyProvider // Pass it directly to the service
            );

            // Act: Call the method
            var result = await service.GetWeatherAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, "London");

            // Assert: Verify the result
            Assert.Equal(20.1, result[0].Temperature);  // Access the first WeatherRecord's Temperature
            Assert.Equal(70, result[0].Humidity);  
        }
    }

}
