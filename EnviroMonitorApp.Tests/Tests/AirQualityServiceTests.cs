using System;
using Xunit;
using NSubstitute;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Tests.Tests
{
    public class AirQualityRecordTests
    {
        [Fact]
        public void AirQualityRecord_HoldsCorrectValues()
        {
            // Arrange
            var timestamp = new DateTime(2025, 4, 25, 14, 0, 0);
            var record = new AirQualityRecord
            {
                Timestamp = timestamp,
                NO2 = 18.5,
                SO2 = 6.2,
                PM25 = 10.3,
                PM10 = 20.1
            };

            // Assert
            Assert.Equal(timestamp, record.Timestamp);
            Assert.Equal(18.5, record.NO2);
            Assert.Equal(6.2, record.SO2);
            Assert.Equal(10.3, record.PM25);
            Assert.Equal(20.1, record.PM10);
        }
    }
}
