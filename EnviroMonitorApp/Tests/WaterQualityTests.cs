using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using EnviroMonitorApp.ViewModels;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests;

// ---------- helper to make fake EA readings ----------
static class Stub
{
    public static WaterReadingRange.Reading R(this double value, DateTime ts) =>
        new WaterReadingRange.Reading
        {
            DateTime = ts.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Value    = value
        };
}

public class WaterQualityTests
{
    #region common ARRANGE helper
    private static (
        EnvironmentalDataApiService svc,
        Mock<IWaterQualityApi>      apiMock)
        BuildService(Dictionary<string, IReadOnlyList<WaterReadingRange.Reading>>? data = null)
    {
        var t1 = DateTime.UtcNow.AddMinutes(-1);
        var t2 = DateTime.UtcNow.AddMinutes(-2);

        data ??= new()
        {
            ["https://environment.data.gov.uk/hydrology/id/measures/E05962A-nitrate-i-subdaily-mgL"]
                = new[] { 10d.R(t1), 11d.R(t2) },
            ["https://environment.data.gov.uk/hydrology/id/measures/E05962A-ph-i-subdaily"]
                = new[] { 7.8.R(t1), 7.7.R(t2) },
            ["https://environment.data.gov.uk/hydrology/id/measures/E05962A-do-i-subdaily-mgL"]
                = new[] { 8.5.R(t1) },
            ["https://environment.data.gov.uk/hydrology/id/measures/E05962A-temp-i-subdaily-C"]
                = new[] { 12.3.R(t1) }
        };

        var apiM = new Mock<IWaterQualityApi>(MockBehavior.Strict);

        foreach (var kvp in data)
        {
            apiM.Setup(a => a.GetRange(
                           kvp.Key,
                           It.IsAny<string>()))
                .ReturnsAsync(new WaterReadingRange
                {
                    Items = kvp.Value.ToArray()
                });
        }

        var airApiM    = new Mock<IAirQualityApi>();
        var weatherApi = new Mock<IWeatherApi>();
        var keys       = new ApiKeyProvider();

        var service = new EnvironmentalDataApiService(
                          airApiM.Object,
                          weatherApi.Object,
                          apiM.Object,
                          keys);

        return (service, apiM);
    }
    #endregion

    // ───────────── actual TESTS ─────────────

    [Fact]
    public async Task DefaultOverload_ReturnsOrderedAndCapped()
    {
        var (svc, _) = BuildService();

        var list = await svc.GetWaterQualityAsync(); // default 24 h

        Assert.True(list.Count <= 10);
        Assert.True(list.SequenceEqual(list.OrderByDescending(r => r.Timestamp)));
    }

    [Fact]
    public async Task MergeLogic_CombinesParameters()
    {
        var (svc, _) = BuildService();

        var rec = (await svc.GetWaterQualityAsync(6)).First();

        Assert.Equal(10,   rec.Nitrate);
        Assert.Equal(7.8,  rec.PH);
        Assert.Equal(8.5,  rec.DissolvedOxygen);
        Assert.Equal(12.3, rec.Temperature);
    }

    [Fact]
    public async Task EmptyApi_YieldsEmptyList()
    {
        var (svc, _) = BuildService(new());

        var list = await svc.GetWaterQualityAsync();

        Assert.Empty(list);
    }

    [Fact]
    public async Task ViewModel_LoadAsync_FillsCollection()
    {
        var reading = new WaterQualityRecord
        {
            Timestamp = DateTime.UtcNow,
            Nitrate   = 9.9
        };

        var svcM = new Mock<IEnvironmentalDataService>();
        svcM.Setup(s => s.GetWaterQualityAsync(24))
            .ReturnsAsync(new List<WaterQualityRecord> { reading });

        var vm = new WaterQualityViewModel(svcM.Object);

        await vm.LoadAsync();

        Assert.Single(vm.WaterQuality);
        Assert.Equal(9.9, vm.WaterQuality.First().Nitrate);
    }
}
