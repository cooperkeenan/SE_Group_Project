using Xunit;
using System.Collections.Generic;
using EnviroMonitorApp.Models;
using NSubstitute;



public class WeatherServiceTests
{
    [Fact]
    public void WeatherRecord_HasCorrectValues()
    {
        // Arrange
        var record = new WeatherRecord
        {
            Timestamp = DateTime.UtcNow,
            Temperature = 16.5,
            Humidity = 70,
            WindSpeed = 4.2
        };

        // Assert
        Assert.Equal(16.5, record.Temperature);
        Assert.Equal(70, record.Humidity);
        Assert.Equal(4.2, record.WindSpeed);
    }
}
