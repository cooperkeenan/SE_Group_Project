using Moq;
using Microsoft.Extensions.Configuration;
using EnviroMonitorApp.Services;
using Xunit;

public class ApiKeyProviderTests
{
    [Fact]
    public void Test_OpenAqKey_Returns_Valid_Key()
    {
        // Arrange: Use the constructor that accepts the key as a parameter
        var apiKeyProvider = new ApiKeyProvider("sample-openaq-key", "sample-openweathermap-key");

        // Act: Retrieve the key
        var key = apiKeyProvider.OpenAqKey;

        // Assert: Ensure the correct sample key is returned
        Assert.Equal("sample-openaq-key", key);
    }

    [Fact]
    public void Test_OpenWeatherMapKey_Returns_Valid_Key()
    {
        // Arrange: Use the constructor that accepts the key as a parameter
        var apiKeyProvider = new ApiKeyProvider("sample-openaq-key", "sample-openweathermap-key");

        // Act: Retrieve the key
        var key = apiKeyProvider.OpenWeatherMap;

        // Assert: Ensure the correct sample key is returned
        Assert.Equal("sample-openweathermap-key", key);
    }
}
