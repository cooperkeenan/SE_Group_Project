// Services/IEnvironmentalDataService.cs
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    public interface IEnvironmentalDataService
    {
        Task<List<AirQualityRecord>> 
            GetAirQualityAsync(DateTime from, DateTime to, string region);

        Task<List<WeatherRecord>> 
            GetWeatherAsync(DateTime from, DateTime to, string region);

        Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(DateTime from, DateTime to, string region);

        // optional overload for “last N hours”
        Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(int hours, string region = "");
        
        Task<List<WaterQualityRecord>> GetHistoricalWaterQualityAsync(
            DateTime from, DateTime to, string region);
        
        
    }
}
