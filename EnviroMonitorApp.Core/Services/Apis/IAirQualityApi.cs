// EnviroMonitorApp/Services/Apis/IAirQualityApi.cs
using Refit;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Interface for accessing air quality data from external APIs.
    /// Provides methods to retrieve location data, latest air quality measurements,
    /// and historical air quality data for specific sensors.
    /// </summary>
    public interface IAirQualityApi
    {
        /// <summary>
        /// Retrieves a list of air quality monitoring locations based on geographic criteria.
        /// </summary>
        /// <param name="iso">The ISO country code (e.g., "GB" for Great Britain)</param>
        /// <param name="latlon">Comma-separated latitude and longitude coordinates (e.g., "51.5074,-0.1278")</param>
        /// <param name="radiusMeters">Search radius in meters from the specified coordinates</param>
        /// <param name="parameterIdsCsv">Comma-separated list of parameter IDs to filter by (e.g., "2,1,7,9" for specific pollutants)</param>
        /// <param name="limit">Maximum number of locations to return</param>
        /// <returns>A response containing matching air quality monitoring locations</returns>
        [Get("/v3/locations")]
        Task<LocationResponse> GetLocations(
            [AliasAs("iso")]           string iso,
            [AliasAs("coordinates")]   string latlon,
            [AliasAs("radius")]        int    radiusMeters,
            [AliasAs("parameters_id")] string parameterIdsCsv,
            [AliasAs("limit")]         int    limit
        );

        /// <summary>
        /// Gets the latest air quality measurements for a specific location.
        /// </summary>
        /// <param name="locationId">The unique identifier of the location</param>
        /// <returns>A response containing the latest measurements for the specified location</returns>
        [Get("/v3/locations/{locationId}/latest")]
        Task<LocationLatestResponse> GetLocationLatest(int locationId);

        /// <summary>
        /// Retrieves historical air quality measurements for a specific sensor within a date range.
        /// </summary>
        /// <param name="sensorId">The unique identifier of the sensor</param>
        /// <param name="datetimeFromUtc">Start date and time in UTC format</param>
        /// <param name="datetimeToUtc">End date and time in UTC format</param>
        /// <param name="limit">Maximum number of measurements to return per page</param>
        /// <param name="page">Page number for paginated results</param>
        /// <returns>A response containing historical measurements for the specified sensor</returns>
        [Get("/v3/sensors/{sensorId}/measurements")]
        Task<MeasurementsResponse> GetSensorMeasurementsAsync(
            [AliasAs("sensorId")]      int    sensorId,
            [AliasAs("datetime_from")] string datetimeFromUtc,
            [AliasAs("datetime_to")]   string datetimeToUtc,
            [AliasAs("limit")]         int    limit,
            [AliasAs("page")]          int    page
        );
    }
}