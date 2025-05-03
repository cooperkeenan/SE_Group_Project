using System;
using System.Collections.Generic;
using EnviroMonitorApp.Services.ChartTransformers;
using Microcharts;
using Xunit;

namespace EnviroMonitorApp.Tests.ChartTransformers
{
    public class LogBinningTransformerTests
    {
        [Fact]
        public void Transform_Compresses_Long_Span_Into_Fewer_Than_MaxLabels()
        {
            // Arrange
            var transformer = new LogBinningTransformer(); // Create the transformer instance

            // Prepare some raw data for testing
            var rawData = new List<(DateTime timestamp, double value)>
            {
                (DateTime.Now.AddDays(-5), 10),
                (DateTime.Now.AddDays(-4), 20),
                (DateTime.Now.AddDays(-3), 30),
                (DateTime.Now.AddDays(-2), 40),
                (DateTime.Now.AddDays(-1), 50)
            };

            DateTime start = DateTime.Now.AddDays(-7); // 7 days ago
            DateTime end = DateTime.Now; // Today

            // Act
            var result = transformer.Transform(rawData, start, end); // Call the method to transform the data

            // Assert
            Assert.NotEmpty(result); // Check that the result is not empty
            Assert.True(result.Count <= transformer.MaxLabels); // Ensure the number of labels is less than or equal to MaxLabels
        }
    }
}
