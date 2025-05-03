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
            // Arrange: Mock WeatherApi
            var mockWeatherApi = new Mock<IWeatherApi>();
            mockWeatherApi.Setup(a => a.GetWeatherAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                          .ReturnsAsync(new WeatherRecord { Temperature = 20.1, Humidity = 70 });

            var mockAirQualityApi = new Mock<IAirQualityApi>();
            var mockWaterQualityApi = new Mock<IWaterQualityApi>();
            var mockApiKeyProvider = new Mock<ApiKeyProvider>();

            // Instantiate the service with required parameters
            var service = new EnvironmentalDataApiService(
                mockAirQualityApi.Object,
                mockWeatherApi.Object,
                mockWaterQualityApi.Object,
                mockApiKeyProvider.Object
            );

            // Act: Call the method
            var result = await service.GetWeatherAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, "London");

            // Assert: Verify the result
            Assert.Equal(20.1, result[0].Temperature);  // Access the first WeatherRecord's Temperature
            Assert.Equal(70, result[0].Humidity);  
        }
    }
}
