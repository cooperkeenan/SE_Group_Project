// EnviroMonitorApp/Services/Apis/LocationLatestResponse.cs
using Refit;


namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Response model for the latest measurements at a specific location.
    /// Used with the GetLocationLatest API endpoint.
    /// </summary>
    public class LocationLatestResponse
    {
        /// <summary>
        /// Metadata about the response, including pagination information.
        /// </summary>
        public Meta Meta { get; set; } = null!;
        
        /// <summary>
        /// Array of latest measurement results from the location.
        /// </summary>
        public LatestResult[] Results { get; set; } = null!;
    }

    /// <summary>
    /// Represents a single measurement result from a sensor at the location.
    /// </summary>
    public class LatestResult
    {
        /// <summary>
        /// The date and time when the measurement was taken, in both UTC and local time.
        /// </summary>
        public DateWrapper Datetime { get; set; } = null!;
        
        /// <summary>
        /// The measured value of the environmental parameter.
        /// </summary>
        public double Value { get; set; }
        
        /// <summary>
        /// The unique identifier of the sensor that took the measurement.
        /// </summary>
        public int SensorsId { get; set; }
        // you can ignore coords here—you're pivoting by parameter
    }
}