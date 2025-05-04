using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.ChartTransformers;
using EnviroMonitorApp.ViewModels;
using Xunit;

namespace EnviroMonitorApp.Tests.ViewModels
{
    public class HistoricalDataViewModelTests
    {
        private readonly Mock<IEnvironmentalDataService> _svc = new();
        private readonly HistoricalDataViewModel _vm;

        public HistoricalDataViewModelTests()
        {
            _svc.Setup(s => s.GetWeatherAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<string>()))
                .ReturnsAsync(new List<WeatherRecord>
                {
                    new() { Timestamp = DateTime.UtcNow, MeanTemp = 12 }
                });

            _vm = new HistoricalDataViewModel(_svc.Object, new LogBinningTransformer());
            _vm.SelectedSensorType = "Weather";
            _vm.SelectedMetric     = "MeanTemp";
        }

        [Fact]
        public async Task LoadDataCommand_Fills_ChartData()
        {
            // Cast to AsyncRelayCommand so we can await ExecuteAsync
            var cmd = (AsyncRelayCommand)_vm.LoadDataCommand;

            // Act
            await cmd.ExecuteAsync(null);

            // Assert
            Assert.Single(_vm.ChartData);
            Assert.False(_vm.NoData);
        }
    }
}
