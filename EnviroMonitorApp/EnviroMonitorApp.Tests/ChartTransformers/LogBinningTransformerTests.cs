using EnviroMonitorApp.ChartTransformers;   // ← removed “.Core.”
using Xunit;

namespace EnviroMonitorApp.Tests.ChartTransformers;

public class LogBinningTransformerTests
{
    [Fact]
    public void Transform_ShouldBucketIntoLogBins()
    {
        var transformer = new LogBinningTransformer(binSize: 10);
        var input = new[] { 1d, 5, 11, 90, 99, 101 };

        var bins = transformer.Transform(input);

        Assert.Equal(3, bins.Count);
        Assert.Equal((1, 5),    bins[0]);
        Assert.Equal((11, 99),  bins[1]);
        Assert.Equal((101,101), bins[2]);
    }
}
