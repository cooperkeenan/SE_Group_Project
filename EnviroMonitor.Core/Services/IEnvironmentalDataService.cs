// IEnvironmentalDataService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

namespace EnviroMonitor.Core.Services
{
    /// <summary>
    /// Defines read operations for weather, air‑quality, and water‑quality
    /// datasets collected by the EnviroMonitor system.
    /// </summary>
    public interface IEnvironmentalDataService
    {
        /// <summary>
        /// Gets the latest weather observations—
        /// temperature, humidity, rainfall, etc.
        /// </summary>
        /// <returns>
        /// A list of <see cref="WeatherRecord"/> items
        /// ordered from newest to oldest.
        /// </returns>
        Task<List<WeatherRecord>> GetWeatherAsync();

        /// <summary>
        /// Gets the most recent air‑quality readings
        /// such as CO₂, PM2.5, and NO₂.
        /// </summary>
        Task<List<AirQualityRecord>> GetAirQualityAsync();

        /// <summary>
        /// Gets water‑quality data—pH, turbidity, dissolved oxygen, and so on.
        /// </summary>
        Task<List<WaterQualityRecord>> GetWaterQualityAsync();
    }
}
