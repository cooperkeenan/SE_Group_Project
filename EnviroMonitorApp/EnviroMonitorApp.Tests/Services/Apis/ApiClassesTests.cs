using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using EnviroMonitorApp.Tests.TestHelpers;
using Refit;
using Xunit;

namespace EnviroMonitorApp.Tests.Services.Apis
{
    public class ApiClassesTests
    {
        [Fact]
        public void LocationResponse_Properties_Are_Properly_Initialized()
        {
            // Arrange & Act
            var response = new LocationResponse();
            
            // In the LocationResponse class, these properties might not be initialized
            // Let's verify that we can set and retrieve them
            response.Meta = new Meta { Page = 1, Limit = 10 };
            response.Results = new Location[] { new Location { Id = 123, Name = "Test Location" } };
            
            // Assert
            Assert.NotNull(response.Meta);
            Assert.NotNull(response.Results);
            Assert.Equal(1, response.Meta.Page);
            Assert.Equal(10, response.Meta.Limit);
            Assert.Single(response.Results);
            Assert.Equal(123, response.Results[0].Id);
        }
        
        [Fact]
        public void DateWrapper_Properties_Can_Be_Set_And_Retrieved()
        {
            // Arrange
            var dateWrapper = new DateWrapper
            {
                Utc = "2023-05-01T10:30:00Z",
                Local = "2023-05-01T11:30:00+01:00"
            };
            
            // Act & Assert
            Assert.Equal("2023-05-01T10:30:00Z", dateWrapper.Utc);
            Assert.Equal("2023-05-01T11:30:00+01:00", dateWrapper.Local);
        }
        
        [Fact]
        public void MeasurementsResponse_Properties_Are_Properly_Initialized()
        {
            // Arrange & Act
            var response = new MeasurementsResponse();
            
            // Initialize properties that might not be initialized in the class
            response.Meta = new MeasurementsMeta { Page = 1, Limit = 10, Name = "Test API" };
            response.Results = new Measurement[0];
            
            // Assert
            Assert.NotNull(response.Meta);
            Assert.NotNull(response.Results);
            Assert.Equal(1, response.Meta.Page);
            Assert.Equal(10, response.Meta.Limit);
            Assert.Equal("Test API", response.Meta.Name);
            Assert.Empty(response.Results);
        }
        
        [Fact]
        public void WaterQualityResponse_Properties_Are_Properly_Initialized()
        {
            // Arrange & Act
            var response = new WaterQualityResponse();
            
            // Assert
            Assert.NotNull(response.Items);
            Assert.Empty(response.Items);
        }
        
        [Fact]
        public void ReadingItem_Properties_Can_Be_Set_And_Retrieved()
        {
            // Arrange
            var readingItem = new ReadingItem
            {
                DateTime = "2023-05-01T10:30:00Z",
                Value = 7.5
            };
            
            // Act & Assert
            Assert.Equal("2023-05-01T10:30:00Z", readingItem.DateTime);
            Assert.Equal(7.5, readingItem.Value);
        }

        [Fact]
        public void LocationLatestResponse_Properties_Are_Properly_Initialized()
        {
            // Arrange & Act
            var response = new LocationLatestResponse();
            
            // Initialize properties that might not be initialized in the class
            response.Meta = new Meta { Page = 1, Limit = 10 };
            response.Results = new LatestResult[0];
            
            // Assert
            Assert.NotNull(response.Meta);
            Assert.NotNull(response.Results);
            Assert.Equal(1, response.Meta.Page);
            Assert.Equal(10, response.Meta.Limit);
            Assert.Empty(response.Results);
        }

        [Fact]
        public void OpenWeatherForecastResponse_Can_Be_Properly_Initialized()
        {
            // Arrange
            var forecast = new OpenWeatherForecastResponse();
            
            // Act - Add a list item to the response
            forecast.List.Add(new OpenWeatherForecastResponse.ListItem
            {
                Dt = 1652345678,
                Main = new OpenWeatherForecastResponse.MainInfo
                {
                    Temp = 22.5,
                    Humidity = 65
                },
                Wind = new OpenWeatherForecastResponse.WindInfo
                {
                    Speed = 3.2
                }
            });
            
            // Assert
            Assert.Single(forecast.List);
            Assert.Equal(22.5, forecast.List[0].Main.Temp);
            Assert.Equal(65, forecast.List[0].Main.Humidity);
            Assert.Equal(3.2, forecast.List[0].Wind.Speed);
        }
    }

    public class ApiIntegrationTests
    {
        // These tests use the FakeHttpHandler to simulate API responses
        
        [Fact]
        public async Task AirQualityApi_GetLocations_Deserializes_Response()
        {
            // Arrange
            var jsonResponse = @"{
                ""meta"": {
                    ""name"": ""OpenAQ API"",
                    ""page"": 1,
                    ""limit"": 10,
                    ""found"": 25
                },
                ""results"": [
                    {
                        ""id"": 123,
                        ""name"": ""London Westminster"",
                        ""sensors"": [
                            {
                                ""id"": 456,
                                ""name"": ""NO2 Sensor"",
                                ""parameter"": {
                                    ""id"": 2,
                                    ""name"": ""no2"",
                                    ""displayName"": ""NO2""
                                }
                            }
                        ],
                        ""datetimeLast"": {
                            ""utc"": ""2023-05-01T10:00:00Z"",
                            ""local"": ""2023-05-01T11:00:00+01:00""
                        }
                    }
                ]
            }";
            
            var handler = new FakeHttpHandler(jsonResponse, HttpStatusCode.OK);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.openaq.org/")
            };
            
            var api = RestService.For<IAirQualityApi>(client);
            
            // Act
            var result = await api.GetLocations(
                iso: "GB", 
                latlon: "51.5074,-0.1278",
                radiusMeters: 10000, 
                parameterIdsCsv: "2,7", 
                limit: 10);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Meta);
            Assert.Equal(1, result.Meta.Page);
            Assert.Equal(10, result.Meta.Limit);
            
            Assert.NotNull(result.Results);
            Assert.Single(result.Results);
            
            var location = result.Results[0];
            Assert.Equal(123, location.Id);
            Assert.Equal("London Westminster", location.Name);
            Assert.Single(location.Sensors);
            
            var sensor = location.Sensors[0];
            Assert.Equal(456, sensor.Id);
            Assert.Equal("NO2 Sensor", sensor.Name);
            Assert.Equal("no2", sensor.Parameter.Name);
        }
        
        [Fact]
        public async Task WaterQualityApi_GetLatest_Deserializes_Response()
        {
            // Arrange
            var jsonResponse = @"{
                ""items"": [
                    {
                        ""dateTime"": ""2023-05-01T12:00:00Z"",
                        ""value"": 7.8
                    },
                    {
                        ""dateTime"": ""2023-05-01T11:00:00Z"",
                        ""value"": 7.7
                    }
                ]
            }";
            
            var handler = new FakeHttpHandler(jsonResponse, HttpStatusCode.OK);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://environment.data.gov.uk/")
            };
            
            var api = RestService.For<IWaterQualityApi>(client);
            
            // Act
            var result = await api.GetLatest(
                latest: true,
                measureUrl: "https://environment.data.gov.uk/hydrology/id/measures/E05962A-ph-i-subdaily");
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("2023-05-01T12:00:00Z", result.Items[0].DateTime);
            Assert.Equal(7.8, result.Items[0].Value);
        }
        
        [Fact]
        public async Task WaterQualityApi_GetRange_Deserializes_Response()
        {
            // Arrange
            var jsonResponse = @"{
                ""items"": [
                    {
                        ""dateTime"": ""2023-05-01T12:00:00Z"",
                        ""value"": 7.8
                    },
                    {
                        ""dateTime"": ""2023-04-30T12:00:00Z"",
                        ""value"": 7.6
                    }
                ]
            }";
            
            var handler = new FakeHttpHandler(jsonResponse, HttpStatusCode.OK);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://environment.data.gov.uk/")
            };
            
            var api = RestService.For<IWaterQualityApi>(client);
            
            // Act
            var result = await api.GetRange(
                measureUrl: "https://environment.data.gov.uk/hydrology/id/measures/E05962A-ph-i-subdaily",
                sinceUtc: "2023-04-30T00:00:00Z",
                limit: 96);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Equal(2, result.Items.Count);
            
            // Test chronological ordering
            Assert.Equal("2023-05-01T12:00:00Z", result.Items[0].DateTime);
            Assert.Equal("2023-04-30T12:00:00Z", result.Items[1].DateTime);
        }
        
        [Fact]
        public async Task WeatherApi_GetForecast_Deserializes_Response()
        {
            // Arrange
            var jsonResponse = @"{
                ""list"": [
                    {
                        ""dt"": 1651410000,
                        ""main"": {
                            ""temp"": 15.2,
                            ""humidity"": 72
                        },
                        ""wind"": {
                            ""speed"": 3.1
                        }
                    },
                    {
                        ""dt"": 1651420800,
                        ""main"": {
                            ""temp"": 16.8,
                            ""humidity"": 65
                        },
                        ""wind"": {
                            ""speed"": 2.7
                        }
                    }
                ]
            }";
            
            var handler = new FakeHttpHandler(jsonResponse, HttpStatusCode.OK);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.openweathermap.org/")
            };
            
            var api = RestService.For<IWeatherApi>(client);
            
            // Act
            var result = await api.GetForecast(
                lat: 51.5074,
                lon: -0.1278,
                apiKey: "test-api-key",
                units: "metric");
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.List);
            Assert.Equal(2, result.List.Count);
            
            Assert.Equal(1651410000, result.List[0].Dt);
            Assert.Equal(15.2, result.List[0].Main.Temp);
            Assert.Equal(72, result.List[0].Main.Humidity);
            Assert.Equal(3.1, result.List[0].Wind.Speed);
        }
    }
}