using System;
using System.Linq;
using EnviroMonitorApp.Services.ChartTransformers;
using Microcharts;
using Xunit;

namespace EnviroMonitorApp.Tests.ChartTransformers;

public class LogBinningTransformerTests
{
    [Fact]
    public void Transform_Compresses_Long_Span_Into_Fewer_Than_MaxLabels()
    {
        // arrange – 365 points, one per day
        var raw = Enumerable.Range(0, 365)
                            .Select(i => (DateTime.UtcNow.AddDays(-i), value: 1d))
                            .ToArray();

        var sut   = new LogBinningTransformer();
        var start = DateTime.UtcNow.AddYears(-1);
        var end   = DateTime.UtcNow;

        // act
        var entries = sut.Transform(raw, start, end);

        // assert – never draw more labels than configured
        Assert.True(entries.Count <= sut.MaxLabels);
        Assert.All(entries, e => Assert.IsType<ChartEntry>(e));
    }
}
