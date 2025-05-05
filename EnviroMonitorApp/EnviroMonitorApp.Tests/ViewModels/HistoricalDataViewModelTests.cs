// EnviroMonitorApp.Tests/ViewModels/HistoricalDataViewModelTests.cs
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.ChartTransformers;
using EnviroMonitorApp.ViewModels;
using Microcharts;
using Xunit;

namespace EnviroMonitorApp.Tests.ViewModels
{
    public class HistoricalDataViewModelTests
    {
        private readonly Mock<IEnvironmentalDataService> _svc         = new();
        private readonly Mock<IChartTransformer>         _transformer = new();
        private readonly HistoricalDataViewModel         _vm;

        public HistoricalDataViewModelTests()
        {
            // ─── Default stubs so constructor-triggered loads succeed ──────────────
            _svc.Setup(s => s.GetAirQualityAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<string>()))
                .ReturnsAsync(new List<AirQualityRecord>());          // empty ok

            _svc.Setup(s => s.GetWaterQualityAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<string>()))
                .ReturnsAsync(new List<WaterQualityRecord>());        // empty ok

            // ─── Weather stub with an in-range timestamp ─────────────────────────
            var sampleTs = new DateTime(2020, 6, 15, 12, 0, 0, DateTimeKind.Utc);

            _svc.Setup(s => s.GetWeatherAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<string>()))
                .ReturnsAsync(new List<WeatherRecord>
                {
                    new() { Timestamp = sampleTs, MeanTemp = 12 }
                });

            // ─── Transformer stub: 1 ChartEntry per value ─────────────────────────
            _transformer.Setup(t => t.Transform(
                    It.IsAny<IEnumerable<(DateTime timestamp, double value)>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
                .Returns((IEnumerable<(DateTime timestamp, double value)> raw, DateTime _, DateTime __) =>
                    raw.Select(x => new ChartEntry((float)x.value)).ToList());

            // ─── Create VM, then switch to Weather/MeanTemp ──────────────────────
            _vm = new HistoricalDataViewModel(_svc.Object, _transformer.Object);
            _vm.SelectedSensorType = "Weather";
            _vm.SelectedMetric     = "MeanTemp";
        }

        [Fact]
        public async Task LoadDataCommand_Fills_ChartData()
        {
            // Wait for any background load to finish
            await SpinWaitAsync(() => !_vm.IsBusy);

            var cmd = (AsyncRelayCommand)_vm.LoadDataCommand;
            await cmd.ExecuteAsync(null);

            Assert.Single(_vm.ChartData);
            Assert.False(_vm.NoData);
        }

        // Helper to wait until condition is true (max 1 s)
        private static async Task SpinWaitAsync(Func<bool> condition, int timeoutMs = 1000)
        {
            var start = DateTime.UtcNow;
            while (!condition())
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                    throw new TimeoutException("Spin wait timed out.");
                await Task.Delay(10);
            }
        }
    }
}
