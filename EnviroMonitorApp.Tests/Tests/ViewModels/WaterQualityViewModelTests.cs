using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests.ViewModels
{
    public class WaterQualityViewModelTests
    {
        [Fact]
        public async Task LoadAsync_PopulatesCollection()
        {
            // Arrange
            var sample = new[]
            {
                new WaterQualityRecord
                {
                    Timestamp = System.DateTime.UtcNow,
                    Nitrate = 1,
                    PH = 7,
                    DissolvedOxygen = 8,
                    Temperature = 9
                }
            };
            var serviceMock = new Mock<IEnvironmentalDataService>();
            serviceMock
                .Setup(s => s.GetWaterQualityAsync(It.IsAny<int>()))
                .ReturnsAsync(new System.Collections.Generic.List<WaterQualityRecord>(sample));

            var vm = new WaterQualityViewModel(serviceMock.Object);

            // Act
            await vm.LoadAsync();

            // Assert
            Assert.Single(vm.WaterQuality);
            Assert.Equal(sample[0], vm.WaterQuality[0]);
        }

        [Fact]
        public async Task LoadAsync_NoOp_WhenNoServiceProvided()
        {
            // Arrange: use design-time ctor
            var vm = new WaterQualityViewModel();
            var initial = new ObservableCollection<WaterQualityRecord>(vm.WaterQuality);

            // Act: should not throw or modify collection
            await vm.LoadAsync();

            // Assert
            Assert.Equal(initial, vm.WaterQuality);
        }
    }
}
