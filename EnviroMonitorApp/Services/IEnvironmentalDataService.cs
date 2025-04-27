using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    public interface IEnvironmentalDataService
    {
        /* Air */
        Task<List<AirQualityRecord>>            GetAirQualityAsync();

        /* Weather */
        Task<IReadOnlyList<WeatherRecord>>      GetWeatherAsync();

        /* Water – latest merged reading (one record) */
        Task<IReadOnlyList<WaterQualityRecord>> GetWaterQualityAsync();
        Task<IReadOnlyList<WaterQualityRecord>> GetWaterQualityAsync(int hours);
    }
}
