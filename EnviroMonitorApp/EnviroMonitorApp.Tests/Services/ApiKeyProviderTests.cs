using System.Collections.Generic;
using EnviroMonitorApp.Services;
using Xunit;

namespace EnviroMonitorApp.Tests.Services;

public class ApiKeyProviderTests
{
    [Fact]
    public void GetKey_ShouldThrow_WhenKeyMissing()
    {
        var provider = new ApiKeyProvider(new Dictionary<string, string>());
        Assert.Throws<KeyNotFoundException>(() => provider.GetKey("OpenWeather"));
    }
}
