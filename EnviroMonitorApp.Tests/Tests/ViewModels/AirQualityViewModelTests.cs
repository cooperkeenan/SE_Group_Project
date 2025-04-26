using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests.ViewModels
{
    public class AirQualityViewModelTests
    {
        [Fact]
        public async Task LoadAsync_PopulatesCollectionAndRaisesPropertyChanged()
        {
            // Arrange
            var sample = new[]
            {
                new AirQualityRecord { Timestamp = DateTime.UtcNow, NO2 = 1, SO2 = 2, PM25 = 3, PM10 = 4 }
            };
            var serviceMock = new Mock<IEnvironmentalDataService>();
            serviceMock
                .Setup(s => s.GetAirQualityAsync())
                .ReturnsAsync(new System.Collections.Generic.List<AirQualityRecord>(sample));

            var vm = new AirQualityViewModel(serviceMock.Object);
            var changed = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.AirQuality))
                    changed = true;
            };

            // Act
            await vm.LoadAsync();

            // Assert
            Assert.True(changed, "PropertyChanged for AirQuality should be raised");
            Assert.Single(vm.AirQuality);
            Assert.Equal(sample[0], vm.AirQuality[0]);
        }

        [Fact]
        public async Task LoadAsync_Throws_WhenServiceThrows()
        {
            // Arrange
            var serviceMock = new Mock<IEnvironmentalDataService>();
            serviceMock
                .Setup(s => s.GetAirQualityAsync())
                .ThrowsAsync(new InvalidOperationException("boom"));

            var vm = new AirQualityViewModel(serviceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => vm.LoadAsync());
        }
    }
}
