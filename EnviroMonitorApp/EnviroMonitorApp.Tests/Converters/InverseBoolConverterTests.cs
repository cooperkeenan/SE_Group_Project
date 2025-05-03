using EnviroMonitorApp.Converters;     // folder now single-level
using Xunit;

namespace EnviroMonitorApp.Tests.Converters;

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter _sut = new();

    [Theory]
    [InlineData(true,  false)]
    [InlineData(false, true)]
    public void Convert_ShouldInvertBoolean(bool input, bool expected)
        => Assert.Equal(expected, (bool)_sut.Convert(input, null, null, null));
}
