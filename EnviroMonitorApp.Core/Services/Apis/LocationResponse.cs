// EnviroMonitorApp/Services/Apis/LocationResponse.cs
using Refit;
using System.Text.Json; 


namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Response model for location information from the air quality API.
    /// Used with the GetLocations API endpoint.
    /// </summary>
    public class LocationResponse
    {
        /// <summary>
        /// Metadata about the response, including pagination information.
        /// </summary>
        public Meta Meta { get; set; } = null!;
        
        /// <summary>
        /// Array of location results matching the query parameters.
        /// </summary>
        public Location[] Results { get; set; } = null!;
    }

    /// <summary>
    /// Metadata information about the API response, including pagination details.
    /// </summary>
    public class Meta
    {
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
        public JsonElement Found { get; set; } 
    }

    /// <summary>
    /// Represents an air quality monitoring location with its associated sensors.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Unique identifier for the location.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Human-readable name of the monitoring location.
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// Array of sensors available at this location.
        /// </summary>
        public Sensor[] Sensors { get; set; } = null!;
        
        /// <summary>
        /// Timestamp of the most recent measurement from this location.
        /// </summary>
        public DateWrapper DatetimeLast { get; set; } = null!;
    }

    /// <summary>
    /// Represents a sensor device at a monitoring location.
    /// </summary>
    public class Sensor
    {
        /// <summary>
        /// Unique identifier for the sensor.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Human-readable name of the sensor.
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// The environmental parameter that this sensor measures.
        /// </summary>
        public Parameter Parameter { get; set; } = null!;
    }

    /// <summary>
    /// Represents an environmental parameter that can be measured.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Unique identifier for the parameter.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Machine-readable name of the parameter (e.g., "no2", "pm25").
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// Human-readable name of the parameter (e.g., "Nitrogen Dioxide", "PM2.5").
        /// </summary>
        public string DisplayName { get; set; } = null!;
    }

    /// <summary>
    /// Wrapper for date/time information in both UTC and local time.
    /// </summary>
    public class DateWrapper
    {
        /// <summary>
        /// The date and time in UTC format.
        /// </summary>
        [AliasAs("utc")]
        public string Utc { get; set; } = null!;
        
        /// <summary>
        /// The date and time in local timezone format.
        /// </summary>
        [AliasAs("local")]
        public string Local { get; set; } = null!;
    }
}