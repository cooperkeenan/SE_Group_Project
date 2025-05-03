using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using EnviroMonitorApp.Services;  // Add this if it's missing


namespace EnviroMonitorApp.Tests.Services
{
    public class ApiKeyProviderTests
    {
        [Fact]
        public void Indexer_Throws_For_Missing_Key()
        {
            var mockConfig = new Mock<IConfiguration>();
            var keyService = new ApiKeyProvider(mockConfig.Object); // pass the mocked IConfiguration

            // This should throw a KeyNotFoundException since "MissingKey" doesn't exist
            Assert.Throws<KeyNotFoundException>(() => _ = keyService["MissingKey"]);
        }

        [Fact]
        public void Indexer_Returns_Valid_Key_For_OpenWeather()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(config => config["ApiKeys:OpenWeatherMap"]).Returns("58ad17345b0c65c317dc9ae88d38634f");

            var keyService = new ApiKeyProvider(mockConfig.Object); // pass the mocked IConfiguration

            // This should return the OpenWeatherMap key
            var apiKey = keyService["OpenWeatherMap"];
            Assert.Equal("58ad17345b0c65c317dc9ae88d38634f", apiKey);
        }
    }
}
