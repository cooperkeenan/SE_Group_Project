// EnviroMonitorApp/Services/Apis/MeasurementsResponse.cs
using Refit;
using System.Text.Json;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Response wrapper for /v3/sensors/{id}/measurements API endpoint.
    /// Contains historical measurement data for a specific sensor.
    /// </summary>
    public class MeasurementsResponse
    {
        /// <summary>
        /// Metadata about the response, including API provider information and pagination details.
        /// </summary>
        public MeasurementsMeta Meta { get; set; } = null!;
        
        /// <summary>
        /// Array of measurement records returned by the API.
        /// </summary>
        public Measurement[] Results { get; set; } = null!;
    }

    /// <summary>
    /// Extended metadata specific to measurement responses.
    /// </summary>
    public class MeasurementsMeta
    {
        /// <summary>
        /// Name of the data provider or organization.
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// Website URL of the data provider.
        /// </summary>
        public string Website { get; set; } = null!;
        
        /// <summary>
        /// Current page number in paginated results.
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// Maximum number of items per page.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Total number of items found.
        /// Uses JsonElement to handle both numeric values and string representations like ">100".
        /// </summary>
        [AliasAs("found")]
        public JsonElement Found { get; set; }
    }

    /// <summary>
    /// Represents a single measurement record from a sensor.
    /// </summary>
    public class Measurement
    {
        /// <summary>
        /// The measured value of the environmental parameter.
        /// </summary>
        public double Value { get; set; }
        
        /// <summary>
        /// Information about the parameter being measured.
        /// </summary>
        public Parameter Parameter { get; set; } = null!;
        
        /// <summary>
        /// Time period information for this measurement.
        /// </summary>
        public PeriodWrapper Period { get; set; } = null!;
    }

    /// <summary>
    /// Wrapper for time period information related to a measurement.
    /// </summary>
    public class PeriodWrapper
    {
        /// <summary>
        /// Human-readable label for the time period (e.g., "1 hour", "24 hour").
        /// </summary>
        public string Label { get; set; } = null!;
        
        /// <summary>
        /// Measurement interval in ISO 8601 duration format (e.g., "PT1H" for 1 hour).
        /// </summary>
        public string Interval { get; set; } = null!;

        /// <summary>
        /// Start date and time of the measurement period.
        /// </summary>
        [AliasAs("datetimeFrom")]
        public DateWrapper DatetimeFrom { get; set; } = null!;

        /// <summary>
        /// End date and time of the measurement period.
        /// </summary>
        [AliasAs("datetimeTo")]
        public DateWrapper DatetimeTo { get; set; } = null!;
    }
}