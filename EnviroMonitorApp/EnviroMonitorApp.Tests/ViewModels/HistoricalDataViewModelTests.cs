using Moq;
using Xunit;
using EnviroMonitorApp.Models;       // Ensure your models are included
using EnviroMonitorApp.ViewModels;   // For HistoricalDataViewModel
using EnviroMonitorApp.Services;     // For IEnvironmentalDataService
using EnviroMonitorApp.Services.ChartTransformers; 

namespace EnviroMonitorApp.Tests.ViewModels
{
    public class HistoricalDataViewModelTests
    {
        private readonly Mock<IEnvironmentalDataService> _svc = new();
        private readonly HistoricalDataViewModel _vm;

        public HistoricalDataViewModelTests()
        {
            // Setup mock data service to return a weather record
            _svc.Setup(s => s.GetWeatherAsync(It.IsAny<DateTime>(),
                                              It.IsAny<DateTime>(),
                                              It.IsAny<string>()))
                .ReturnsAsync(new List<WeatherRecord>
                {
                    new() { Timestamp = DateTime.UtcNow, MeanTemp = 12 }
                });

            _vm = new HistoricalDataViewModel(_svc.Object, new LogBinningTransformer());
            _vm.SelectedSensorType = "Weather";
            _vm.SelectedMetric = "MeanTemp";
        }

        [Fact]
        public async Task LoadDataCommand_Fills_ChartData()
        {
            // Execute the command to load data
            _vm.LoadDataCommand.Execute(null);

            // Assert that ChartData contains one item and NoData is false
            Assert.Single(_vm.ChartData);
            Assert.False(_vm.NoData);
        }
    }
}
