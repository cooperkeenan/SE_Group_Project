using Xunit;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.Tests.Services
{
    public class ApiKeyProviderTests
    {
        [Fact]
        public void Constructor_WithoutParameters_UsesDefaultKeys()
        {
            // Arrange & Act
            var provider = new ApiKeyProvider();
            
            // Assert
            Assert.Equal("sample-openaq-key", provider.OpenAqKey);
            Assert.Equal("sample-openweathermap-key", provider.OpenWeatherMap);
        }
        
        [Fact]
        public void Constructor_WithParameters_SetsCustomKeys()
        {
            // Arrange
            var customAqKey = "custom-openaq-key";
            var customWeatherKey = "custom-weathermap-key";
            
            // Act
            var provider = new ApiKeyProvider(customAqKey, customWeatherKey);
            
            // Assert
            Assert.Equal(customAqKey, provider.OpenAqKey);
            Assert.Equal(customWeatherKey, provider.OpenWeatherMap);
        }
        
        [Fact]
        public void Constructor_WithNullAqKey_UsesDefaultAqKey()
        {
            // Arrange
            var customWeatherKey = "custom-weathermap-key";
            
            // Act
            var provider = new ApiKeyProvider(null, customWeatherKey);
            
            // Assert
            Assert.Equal("sample-openaq-key", provider.OpenAqKey);
            Assert.Equal(customWeatherKey, provider.OpenWeatherMap);
        }
        
        [Fact]
        public void Constructor_WithNullWeatherKey_UsesDefaultWeatherKey()
        {
            // Arrange
            var customAqKey = "custom-openaq-key";
            
            // Act
            var provider = new ApiKeyProvider(customAqKey, null);
            
            // Assert
            Assert.Equal(customAqKey, provider.OpenAqKey);
            Assert.Equal("sample-openweathermap-key", provider.OpenWeatherMap);
        }
        
        [Fact]
        public void Constructor_WithEmptyKeys_UsesDefaultKeys()
        {
            // Arrange
            var emptyAqKey = string.Empty;
            var emptyWeatherKey = string.Empty;
            
            // Act
            var provider = new ApiKeyProvider(emptyAqKey, emptyWeatherKey);
            
            // Assert
            Assert.Equal("sample-openaq-key", provider.OpenAqKey);
            Assert.Equal("sample-openweathermap-key", provider.OpenWeatherMap);
        }


        [Fact]
        public void ApiKeyProperties_ReturnCorrectValues()
        {
            // Arrange
            string customAqKey = "custom-openaq-key";
            string customWeatherKey = "custom-weathermap-key";
            var provider = new ApiKeyProvider(customAqKey, customWeatherKey);
            
            // Act & Assert
            Assert.Equal(customAqKey, provider.OpenAqKey);
            Assert.Equal(customWeatherKey, provider.OpenWeatherMap);
        }

        [Fact]
        public void Constructor_WithNullParams_SetsDefaultValues()
        {
            // Arrange & Act
            var provider = new ApiKeyProvider(null, null);
            
            // Assert
            Assert.Equal("sample-openaq-key", provider.OpenAqKey);
            Assert.Equal("sample-openweathermap-key", provider.OpenWeatherMap);
        }

        [Fact]
        public void Constructor_WithEmptyStrings_SetsDefaultValues()
        {
            // Arrange & Act
            var provider = new ApiKeyProvider("", "");
            
            // Assert
            Assert.Equal("sample-openaq-key", provider.OpenAqKey);
            Assert.Equal("sample-openweathermap-key", provider.OpenWeatherMap);
        }
    }
}