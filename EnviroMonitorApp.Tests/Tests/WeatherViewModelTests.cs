using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class WeatherViewModelTests
    {
        [Fact]
        public async Task LoadAsync_PopulatesForecastAndRaisesPropertyChanged()
        {
            // Arrange: prepare fake weather data
            var fakeData = new List<WeatherRecord>
            {
                new WeatherRecord 
                { 
                    Timestamp = new DateTime(2025,4,25,10,0,0), 
                    Temperature = 20.5, 
                    Humidity = 65, 
                    WindSpeed = 3.2 
                },
                new WeatherRecord 
                { 
                    Timestamp = new DateTime(2025,4,25,11,0,0), 
                    Temperature = 21.0, 
                    Humidity = 60, 
                    WindSpeed = 4.0 
                }
            };

            var svcMock = new Mock<IEnvironmentalDataService>();
            svcMock
                .Setup(s => s.GetWeatherAsync())
                .ReturnsAsync(fakeData);

            var vm = new WeatherViewModel(svcMock.Object);

            // capture PropertyChanged
            string? changedProp = null;
            vm.PropertyChanged += (_, e) => changedProp = e.PropertyName;

            // Act
            await vm.LoadAsync();

            // Assert: Forecast contains exactly our fake items
            Assert.Equal(2, vm.Forecast.Count);
            Assert.Equal(fakeData[0], vm.Forecast[0]);
            Assert.Equal(fakeData[1], vm.Forecast[1]);

            // Assert: PropertyChanged was raised for "Forecast"
            Assert.Equal(nameof(vm.Forecast), changedProp);
        }
    }
}
