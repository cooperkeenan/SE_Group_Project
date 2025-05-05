using System;
using System.Collections.Generic;
using EnviroMonitorApp.Models;
using Xunit;

namespace EnviroMonitorApp.Tests.Models
{
    public class AirQualityRecordTests
    {
        [Fact]
        public void AirQualityRecord_Properties_Can_Be_Set_And_Retrieved()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var record = new AirQualityRecord
            {
                Timestamp = timestamp,
                NO2 = 10.5,
                SO2 = 5.3,
                PM25 = 8.2,
                PM10 = 15.4,
                Pm25 = 8.2,  // Duplicate property
                Pm10 = 15.4, // Duplicate property
                Category = "Good"
            };
            
            // Act & Assert
            Assert.Equal(timestamp, record.Timestamp);
            Assert.Equal(10.5, record.NO2);
            Assert.Equal(5.3, record.SO2);
            Assert.Equal(8.2, record.PM25);
            Assert.Equal(15.4, record.PM10);
            Assert.Equal(8.2, record.Pm25);
            Assert.Equal(15.4, record.Pm10);
            Assert.Equal("Good", record.Category);
        }
    }
    
    public class WaterQualityRecordTests
    {
        [Fact]
        public void WaterQualityRecord_Properties_Can_Be_Set_And_Retrieved()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var record = new WaterQualityRecord
            {
                Timestamp = timestamp,
                Nitrate = 8.5,
                PH = 7.2,
                DissolvedOxygen = 9.3,
                Temperature = 18.7
            };
            
            // Act & Assert
            Assert.Equal(timestamp, record.Timestamp);
            Assert.Equal(8.5, record.Nitrate);
            Assert.Equal(7.2, record.PH);
            Assert.Equal(9.3, record.DissolvedOxygen);
            Assert.Equal(18.7, record.Temperature);
        }
        
        [Fact]
        public void WaterQualityRecord_Properties_Can_Be_Null()
        {
            // Arrange
            var record = new WaterQualityRecord
            {
                Timestamp = DateTime.UtcNow,
                // Leave other properties as null
            };
            
            // Act & Assert
            Assert.Null(record.Nitrate);
            Assert.Null(record.PH);
            Assert.Null(record.DissolvedOxygen);
            Assert.Null(record.Temperature);
        }
    }
    
    public class WeatherRecordTests
    {
        [Fact]
        public void WeatherRecord_Properties_Can_Be_Set_And_Retrieved()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var record = new WeatherRecord
            {
                Timestamp = timestamp,
                Temperature = 20.1,
                Humidity = 70.0,
                WindSpeed = 5.5,
                CloudCover = 70.0,
                Sunshine = 5.5,
                GlobalRadiation = 150.2,
                MaxTemp = 25.5,
                MeanTemp = 22.3,
                MinTemp = 18.1,
                Precipitation = 0.0,
                Pressure = 1013.2,
                SnowDepth = 0.0
            };
            
            // Act & Assert
            Assert.Equal(timestamp, record.Timestamp);
            Assert.Equal(20.1, record.Temperature);
            Assert.Equal(70.0, record.Humidity);
            Assert.Equal(5.5, record.WindSpeed);
            Assert.Equal(70.0, record.CloudCover);
            Assert.Equal(5.5, record.Sunshine);
            Assert.Equal(150.2, record.GlobalRadiation);
            Assert.Equal(25.5, record.MaxTemp);
            Assert.Equal(22.3, record.MeanTemp);
            Assert.Equal(18.1, record.MinTemp);
            Assert.Equal(0.0, record.Precipitation);
            Assert.Equal(1013.2, record.Pressure);
            Assert.Equal(0.0, record.SnowDepth);
        }
    }
    
    public class OpenWeatherForecastResponseTests
    {
        [Fact]
        public void OpenWeatherForecastResponse_List_Is_Initialized()
        {
            // Arrange & Act
            var response = new OpenWeatherForecastResponse();
            
            // Assert
            Assert.NotNull(response.List);
            Assert.Empty(response.List);
        }
        
        [Fact]
        public void OpenWeatherForecastResponse_Can_Add_ListItems()
        {
            // Arrange
            var response = new OpenWeatherForecastResponse();
            var item1 = new OpenWeatherForecastResponse.ListItem
            {
                Dt = 1651410000,
                Main = new OpenWeatherForecastResponse.MainInfo
                {
                    Temp = 15.2,
                    Humidity = 72
                },
                Wind = new OpenWeatherForecastResponse.WindInfo
                {
                    Speed = 3.1
                }
            };
            
            var item2 = new OpenWeatherForecastResponse.ListItem
            {
                Dt = 1651420800,
                Main = new OpenWeatherForecastResponse.MainInfo
                {
                    Temp = 16.8,
                    Humidity = 65
                },
                Wind = new OpenWeatherForecastResponse.WindInfo
                {
                    Speed = 2.7
                }
            };
            
            // Act
            response.List.Add(item1);
            response.List.Add(item2);
            
            // Assert
            Assert.Equal(2, response.List.Count);
            Assert.Equal(15.2, response.List[0].Main.Temp);
            Assert.Equal(16.8, response.List[1].Main.Temp);
        }
    }
}