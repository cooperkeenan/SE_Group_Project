using EnviroMonitorApp.Models;  // Add this import
using System.Threading.Tasks;
using Refit;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Refit interface for OpenWeather’s forecast endpoint
    /// </summary>
    public interface IWeatherApi
    {
        [Get("/data/2.5/forecast")]
        Task<OpenWeatherForecastResponse> GetForecast(
            [AliasAs("lat")] double lat,
            [AliasAs("lon")] double lon,
            [AliasAs("appid")] string apiKey,
            [AliasAs("units")] string units
        );
    
        Task<WeatherRecord> GetWeatherAsync(DateTime startDate, DateTime endDate, string region);
    }
}
