// Services/IEnvironmentalDataService.cs
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Interface for environmental data services that provide access to air quality,
    /// weather, and water quality data from various sources.
    /// </summary>
    public interface IEnvironmentalDataService
    {
        /// <summary>
        /// Retrieves air quality data for a specified time period and region.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier (e.g., "London").</param>
        /// <returns>A list of air quality records for the specified period and region.</returns>
        Task<List<AirQualityRecord>> 
            GetAirQualityAsync(DateTime from, DateTime to, string region);

        /// <summary>
        /// Retrieves weather data for a specified time period and region.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier (e.g., "London").</param>
        /// <returns>A list of weather records for the specified period and region.</returns>
        Task<List<WeatherRecord>> 
            GetWeatherAsync(DateTime from, DateTime to, string region);

        /// <summary>
        /// Retrieves water quality data for a specified time period and region.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of water quality records for the specified period and region.</returns>
        Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(DateTime from, DateTime to, string region);

        /// <summary>
        /// Retrieves water quality data for a specified number of hours up to the present.
        /// </summary>
        /// <param name="hours">Number of hours of data to retrieve.</param>
        /// <param name="region">Geographic region identifier (optional).</param>
        /// <returns>A list of water quality records for the specified duration.</returns>
        Task<List<WaterQualityRecord>> 
            GetWaterQualityAsync(int hours, string region = "");
        
        /// <summary>
        /// Retrieves historical water quality data for a specified time period and region.
        /// </summary>
        /// <param name="from">Start date and time.</param>
        /// <param name="to">End date and time.</param>
        /// <param name="region">Geographic region identifier.</param>
        /// <returns>A list of historical water quality records for the specified period and region.</returns>
        Task<List<WaterQualityRecord>> GetHistoricalWaterQualityAsync(
            DateTime from, DateTime to, string region);
    }
}