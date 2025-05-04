using System;
using System.Globalization;
using EnviroMonitorApp.Converters;
using Xunit;

namespace EnviroMonitorApp.Tests.Converters
{
    public class InverseBoolConverterTests
    {
        private readonly InverseBoolConverter _sut = new();

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Convert_Flips_Boolean(bool input, bool expected)
        {
            // Act
            var result = _sut.Convert(input, typeof(bool), null!, CultureInfo.InvariantCulture);
            
            // Assert
            Assert.Equal(expected, result);
            Assert.IsType<bool>(result);
        }
        
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ConvertBack_Flips_Boolean(bool input, bool expected)
        {
            // Act
            var result = _sut.ConvertBack(input, typeof(bool), null!, CultureInfo.InvariantCulture);
            
            // Assert
            Assert.Equal(expected, result);
            Assert.IsType<bool>(result);
        }
        
        [Fact]
        public void Convert_WithNonBooleanInput_ReturnsInputUnchanged()
        {
            // Arrange
            var input = "not a boolean";
            
            // Act
            var result = _sut.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);
            
            // Assert
            Assert.Same(input, result);
        }
        
        [Fact]
        public void ConvertBack_WithNonBooleanInput_ReturnsInputUnchanged()
        {
            // Arrange
            var input = 42;
            
            // Act
            var result = _sut.ConvertBack(input, typeof(int), null!, CultureInfo.InvariantCulture);
            
            // Assert
            Assert.Equal(input, result);
        }
    }
}