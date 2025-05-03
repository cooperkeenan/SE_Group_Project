using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Tests.TestHelpers;
using Xunit;

namespace EnviroMonitorApp.Tests.Services;

public class EnvironmentalDataApiServiceTests
{
    [Fact]
    public async Task GetLatestAsync_ShouldReturnStronglyTypedModel()
    {
        using var handler = new FakeHttpHandler(SampleData.LatestWeatherJson, HttpStatusCode.OK);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://fake") };
        var sut    = new EnvironmentalDataApiService(client);

        var data = await sut.GetLatestAsync("london");

        Assert.Equal(21.3, data.Temperature);
        Assert.Equal(60,   data.Humidity);
    }
}
