using Refit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Response object for water quality data containing a collection of reading items.
    /// </summary>
    public class WaterQualityResponse
    {
        /// <summary>
        /// Collection of water quality reading items.
        /// </summary>
        public List<ReadingItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Represents a single water quality reading with timestamp and measured value.
    /// </summary>
    public class ReadingItem
    {
        /// <summary>
        /// The date and time when the reading was taken, in ISO 8601 format.
        /// </summary>
        public string DateTime { get; set; } = null!;
        
        /// <summary>
        /// The measured value of the water quality parameter.
        /// </summary>
        public double Value    { get; set; }
    }

    /// <summary>
    /// Interface for accessing water quality data from external APIs.
    /// Provides methods to retrieve latest readings and historical data within specified timeframes.
    /// </summary>
    public interface IWaterQualityApi
    {
        /// <summary>
        /// Gets the latest water quality readings for a specific measurement parameter.
        /// </summary>
        /// <param name="latest">Flag to indicate whether to retrieve only the latest reading</param>
        /// <param name="measureUrl">URL identifying the specific water quality measure to retrieve</param>
        /// <returns>A response containing the latest water quality readings</returns>
        [Get("/hydrology/data/readings.json")]
        Task<WaterQualityResponse> GetLatest(
            [AliasAs("latest")]  bool   latest,
            [AliasAs("measure")] string measureUrl
        );

        /// <summary>
        /// Gets water quality readings within a specified time range for a particular measurement parameter.
        /// </summary>
        /// <param name="measureUrl">URL identifying the specific water quality measure to retrieve</param>
        /// <param name="sinceUtc">Start date and time in UTC format</param>
        /// <param name="limit">Maximum number of readings to return (default: 96)</param>
        /// <returns>A response containing water quality readings within the specified time range</returns>
        [Get("/hydrology/data/readings.json")]
        Task<WaterQualityResponse> GetRange(
            [AliasAs("measure")] string measureUrl,
            [AliasAs("since")]   string sinceUtc,
            [AliasAs("_limit")]  int    limit = 96
        );  
    }
}