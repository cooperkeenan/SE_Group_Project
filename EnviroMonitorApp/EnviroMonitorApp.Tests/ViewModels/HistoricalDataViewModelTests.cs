// ViewModels/HistoricalDataViewModelTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Models;                 // WeatherRecord, etc.
using EnviroMonitorApp.Services;              // IEnvironmentalDataService
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests.ViewModels;

public class HistoricalDataViewModelTests
{
    private readonly Mock<IEnvironmentalDataService> _data = new();
    private readonly HistoricalDataViewModel _vm;

    public HistoricalDataViewModelTests()
    {
        _data.Setup(d => d.GetHistoricalAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
             .ReturnsAsync(new List<WeatherRecord>
             {
                 new() { Date = DateOnly.FromDateTime(DateTime.Today), Temperature = 12 }
             });

        _vm = new HistoricalDataViewModel(_data.Object);
    }

    [Fact]
    public async Task LoadCommand_ShouldPopulateCollection()
    {
        await _vm.LoadCommand.ExecuteAsync(null);

        Assert.Single(_vm.Records);
        Assert.Equal(12, _vm.Records.First().Temperature);
        Assert.False(_vm.IsBusy);
    }
}
