using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using Xunit;

namespace EnviroMonitorApp.Tests.Services
{
    public class EnvironmentalDataApiServiceTests
    {
        private readonly Mock<IAirQualityApi> _mockAirApi = new();
        private readonly Mock<IWeatherApi> _mockWeatherApi = new();
        private readonly Mock<IWaterQualityApi> _mockWaterApi = new();
        private readonly ApiKeyProvider _keys = new ApiKeyProvider("test-aq-key", "test-weather-key");

        [Fact]
        public async Task GetWeatherAsync_Returns_Model_From_Api()
        {
            // Arrange: mock the HTTP API
            _mockWeatherApi
                .Setup(api => api.GetForecast(
                    It.IsAny<double>(),
                    It.IsAny<double>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new OpenWeatherForecastResponse
                {
                    List = new List<OpenWeatherForecastResponse.ListItem>
                    {
                        new OpenWeatherForecastResponse.ListItem
                        {
                            Dt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Main = new OpenWeatherForecastResponse.MainInfo
                            {
                                Temp = 20.1,
                                Humidity = 70
                            },
                            Wind = new OpenWeatherForecastResponse.WindInfo
                            {
                                Speed = 5.5
                            }
                        }
                    }
                });

            var svc = new EnvironmentalDataApiService(
                _mockAirApi.Object,
                _mockWeatherApi.Object,
                _mockWaterApi.Object,
                _keys
            );

            // Act
            var result = await svc.GetWeatherAsync(
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow,
                "London"
            );

            // Assert
            Assert.Single(result);
            var wr = result[0];
            Assert.Equal(20.1, wr.Temperature);
            Assert.Equal(70, wr.Humidity);
            Assert.Equal(5.5, wr.WindSpeed);
        }

        [Fact]
        public async Task GetAirQualityAsync_Returns_Model_From_Api()
        {
            // Arrange: Create sample location response
            var locationResponse = new LocationResponse
            {
                Meta = new Meta { Page = 1, Limit = 10 },
                Results = new Location[]
                {
                    new Location
                    {
                        Id = 123,
                        Name = "Test Location",
                        Sensors = new Sensor[]
                        {
                            new Sensor
                            {
                                Id = 456,
                                Name = "NO2 Sensor",
                                Parameter = new Parameter { Id = 2, Name = "no2", DisplayName = "NO2" }
                            },
                            new Sensor
                            {
                                Id = 457,
                                Name = "PM25 Sensor",
                                Parameter = new Parameter { Id = 7, Name = "pm25", DisplayName = "PM2.5" }
                            }
                        },
                        DatetimeLast = new DateWrapper { Utc = DateTime.UtcNow.ToString("o") }
                    }
                }
            };

            // Create sample measurements response
            var measurementsResponse = new MeasurementsResponse
            {
                Meta = new MeasurementsMeta { Page = 1, Limit = 100, Name = "Test API" },
                Results = new Measurement[]
                {
                    new Measurement
                    {
                        Value = 15.2,
                        Parameter = new Parameter { Name = "no2" },
                        Period = new PeriodWrapper
                        {
                            Label = "1 hour",
                            Interval = "hourly",
                            DatetimeFrom = new DateWrapper { Utc = DateTime.UtcNow.AddHours(-1).ToString("o") },
                            DatetimeTo = new DateWrapper { Utc = DateTime.UtcNow.ToString("o") }
                        }
                    },
                    new Measurement
                    {
                        Value = 8.7,
                        Parameter = new Parameter { Name = "pm25" },
                        Period = new PeriodWrapper
                        {
                            Label = "1 hour",
                            Interval = "hourly",
                            DatetimeFrom = new DateWrapper { Utc = DateTime.UtcNow.AddHours(-1).ToString("o") },
                            DatetimeTo = new DateWrapper { Utc = DateTime.UtcNow.ToString("o") }
                        }
                    }
                }
            };

            // Setup the air API mock
            _mockAirApi
                .Setup(api => api.GetLocations(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(locationResponse);

            _mockAirApi
                .Setup(api => api.GetSensorMeasurementsAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(measurementsResponse);

            // Create service with mocks
            var svc = new EnvironmentalDataApiService(
                _mockAirApi.Object,
                _mockWeatherApi.Object,
                _mockWaterApi.Object,
                _keys
            );

            // Act
            var from = DateTime.UtcNow.AddHours(-2);
            var to = DateTime.UtcNow;
            var result = await svc.GetAirQualityAsync(from, to, "London");

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, r => r.NO2 != null && r.NO2 > 0);
            Assert.Contains(result, r => r.PM25 != null && r.PM25 > 0);
            
            // Verify the API was called
            _mockAirApi.Verify(api => api.GetLocations(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>()), 
                Times.Once);
            
            _mockAirApi.Verify(api => api.GetSensorMeasurementsAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()), 
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetAirQualityAsync_Returns_Cached_Data_When_Available()
        {
            // Arrange
            // Setup mock same as previous test
            var locationResponse = new LocationResponse
            {
                Meta = new Meta { Page = 1, Limit = 10 },
                Results = new Location[]
                {
                    new Location
                    {
                        Id = 123,
                        Name = "Test Location",
                        Sensors = new Sensor[]
                        {
                            new Sensor
                            {
                                Id = 456,
                                Name = "NO2 Sensor",
                                Parameter = new Parameter { Id = 2, Name = "no2", DisplayName = "NO2" }
                            }
                        },
                        DatetimeLast = new DateWrapper { Utc = DateTime.UtcNow.ToString("o") }
                    }
                }
            };

            var measurementsResponse = new MeasurementsResponse
            {
                Meta = new MeasurementsMeta { Page = 1, Limit = 100, Name = "Test API" },
                Results = new Measurement[]
                {
                    new Measurement
                    {
                        Value = 15.2,
                        Parameter = new Parameter { Name = "no2" },
                        Period = new PeriodWrapper
                        {
                            Label = "1 hour",
                            Interval = "hourly",
                            DatetimeFrom = new DateWrapper { Utc = DateTime.UtcNow.AddHours(-1).ToString("o") },
                            DatetimeTo = new DateWrapper { Utc = DateTime.UtcNow.ToString("o") }
                        }
                    }
                }
            };

            _mockAirApi
                .Setup(api => api.GetLocations(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(locationResponse);

            _mockAirApi
                .Setup(api => api.GetSensorMeasurementsAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(measurementsResponse);

            var svc = new EnvironmentalDataApiService(
                _mockAirApi.Object,
                _mockWeatherApi.Object,
                _mockWaterApi.Object,
                _keys
            );

            var from = DateTime.UtcNow.AddHours(-2);
            var to = DateTime.UtcNow;

            // Act - Call the service twice
            var result1 = await svc.GetAirQualityAsync(from, to, "London");
            
            // Reset the mock to verify it's not called again
            _mockAirApi.Invocations.Clear();
            
            var result2 = await svc.GetAirQualityAsync(from, to, "London");

            // Assert
            Assert.NotEmpty(result2);
            
            // Verify the API was NOT called the second time (using cache)
            _mockAirApi.Verify(api => api.GetLocations(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>()), 
                Times.Never);
        }

        [Fact]
        public async Task GetWaterQualityAsync_Returns_Data_From_Api()
        {
            // Arrange
            var waterResponse = new WaterQualityResponse
            {
                Items = new List<ReadingItem>
                {
                    new ReadingItem
                    {
                        DateTime = DateTime.UtcNow.ToString("o"),
                        Value = 7.2
                    }
                }
            };

            _mockWaterApi
                .Setup(api => api.GetRange(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(waterResponse);

            var svc = new EnvironmentalDataApiService(
                _mockAirApi.Object,
                _mockWeatherApi.Object,
                _mockWaterApi.Object,
                _keys
            );

            // Act
            var result = await svc.GetWaterQualityAsync(24, "London");

            // Assert
            Assert.NotEmpty(result);
            
            // Verify the water API was called for each measure type (nitrate, ph, oxygen, temp)
            _mockWaterApi.Verify(api => api.GetRange(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()), 
                Times.AtLeast(4));
        }
    }
}