using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.ChartTransformers;

namespace EnviroMonitorApp.Tests.ViewModels
{
    public class HistoricalDataViewModelTests
    {
        [Fact]
        public async Task LoadDataCommand_Fills_ChartData()
        {
            // Arrange: a fake service that returns one WeatherRecord
            var svcMock = new Mock<IEnvironmentalDataService>();
            svcMock
              .Setup(s => s.GetWeatherAsync(
                  It.IsAny<DateTime>(),
                  It.IsAny<DateTime>(),
                  It.IsAny<string>()))
              .ReturnsAsync(new List<WeatherRecord>
              {
                  new() { Timestamp = DateTime.UtcNow, MeanTemp = 12 }
              });

            // Build the VM, pick Weather → MeanTemp
            var vm = new HistoricalDataViewModel(svcMock.Object, new LogBinningTransformer());
            vm.SelectedSensorType = "Weather";
            vm.SelectedMetric     = "MeanTemp";

            // Act: run the AsyncRelayCommand
            await vm.LoadDataCommand.ExecuteAsync(null);

            // Assert: one entry, and NoData==false
            Assert.Single(vm.ChartData);
            Assert.False(vm.NoData);
        }
    }
}
