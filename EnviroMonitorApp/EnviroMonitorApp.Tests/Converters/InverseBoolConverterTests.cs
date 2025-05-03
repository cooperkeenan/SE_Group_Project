using EnviroMonitorApp.Converters;
using Xunit;

namespace EnviroMonitorApp.Tests.Converters;

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter _sut = new();

    [Theory]
    [InlineData(true,  false)]
    [InlineData(false, true)]
    public void Convert_Flips_Boolean(bool input, bool expected)
    {
        var result = (bool)_sut.Convert(input, null!, null!, null!);
        Assert.Equal(expected, result);
    }
}
