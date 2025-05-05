using EnviroMonitorApp.Models;  // Add this import
using System.Threading.Tasks;
using Refit;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Interface for accessing weather data from OpenWeather and other weather APIs.
    /// Provides methods to retrieve weather forecasts and historical weather data.
    /// </summary>
    public interface IWeatherApi
    {
        /// <summary>
        /// Retrieves a 5-day weather forecast with 3-hour step from OpenWeather API.
        /// </summary>
        /// <param name="lat">Latitude coordinate</param>
        /// <param name="lon">Longitude coordinate</param>
        /// <param name="apiKey">OpenWeatherMap API key</param>
        /// <param name="units">Units of measurement (e.g., "metric", "imperial")</param>
        /// <returns>A response containing forecast data for the specified location</returns>
        [Get("/data/2.5/forecast")]
        Task<OpenWeatherForecastResponse> GetForecast(
            [AliasAs("lat")] double lat,
            [AliasAs("lon")] double lon,
            [AliasAs("appid")] string apiKey,
            [AliasAs("units")] string units
        );
    
        /// <summary>
        /// Retrieves historical weather data for a specific location and time period.
        /// </summary>
        /// <param name="startDate">The start date of the requested time period</param>
        /// <param name="endDate">The end date of the requested time period</param>
        /// <param name="region">The geographic region identifier (e.g., "London")</param>
        /// <returns>A WeatherRecord containing historical weather data</returns>
        Task<WeatherRecord> GetWeatherAsync(DateTime startDate, DateTime endDate, string region);
    }
}