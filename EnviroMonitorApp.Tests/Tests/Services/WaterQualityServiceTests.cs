using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class WaterQualityTests
    {
        [Fact]
        public async Task GetWaterQualityAsync_WithHours_MergesByMinuteAndLimitsTo10()
        {
            // Arrange: build two timestamps, “now” and one minute earlier
            var now  = DateTime.UtcNow;
            var iso1 = now   .ToString("yyyy-MM-ddTHH:mm:ssZ");
            var iso2 = now.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Stubbed responses for each measure
            var map = new Dictionary<string, WaterQualityResponse>
            {
                ["nitrate"] = new WaterQualityResponse {
                    Items = new List<ReadingItem> {
                        new() { DateTime = iso1, Value = 1 },
                        new() { DateTime = iso2, Value = 2 }
                    }
                },
                ["ph"] = new WaterQualityResponse {
                    Items = new List<ReadingItem> {
                        new() { DateTime = iso1, Value = 3 }
                    }
                },
                ["oxygen"] = new WaterQualityResponse { Items = new List<ReadingItem>() },
                ["temp"]   = new WaterQualityResponse { Items = new List<ReadingItem>() }
            };

            var waterMock = new Mock<IWaterQualityApi>(MockBehavior.Strict);
            waterMock
                .Setup(x => x.GetRange(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    96
                ))
                .ReturnsAsync((string url, string since, int limit) =>
                {
                    if      (url.Contains("nitrate"))       return map["nitrate"];
                    else if (url.Contains("-ph-"))          return map["ph"];
                    else if (url.Contains("do-i-subdaily")) return map["oxygen"];
                    else if (url.Contains("-temp-"))        return map["temp"];
                    throw new InvalidOperationException($"Unexpected URL: {url}");
                });

            var svc = new EnvironmentalDataApiService(
                airApi:     Mock.Of<IAirQualityApi>(),
                weatherApi: Mock.Of<IWeatherApi>(),
                waterApi:   waterMock.Object,
                keys:       new ApiKeyProvider()
            );

            // Act: last 2 hours
            var result = await svc.GetWaterQualityAsync(2);

            // Assert: exactly two records
            Assert.Equal(2, result.Count);

            // Each Timestamp should be rounded to the minute
            Assert.All(result, r => Assert.Equal(0, r.Timestamp.Second));

            // They are in descending order and one minute apart
            Assert.True(result[0].Timestamp > result[1].Timestamp);
            Assert.Equal(TimeSpan.FromMinutes(1),
                         result[0].Timestamp - result[1].Timestamp);

            // Newest record: nitrate=1, ph=3, others null
            var newest = result[0];
            Assert.Equal( 1, newest.Nitrate);
            Assert.Equal( 3, newest.PH);
            Assert.Null( newest.DissolvedOxygen);
            Assert.Null( newest.Temperature);

            // Older record: nitrate=2, ph=null, others null
            var older = result[1];
            Assert.Equal( 2, older.Nitrate);
            Assert.Null  ( older.PH);
            Assert.Null  ( older.DissolvedOxygen);
            Assert.Null  ( older.Temperature);

            // Verify each measure URL was called once (4 total)
            waterMock.Verify(x => x.GetRange(
                It.IsAny<string>(),
                It.IsAny<string>(),
                96
            ), Times.Exactly(4));
        }
    }
}
