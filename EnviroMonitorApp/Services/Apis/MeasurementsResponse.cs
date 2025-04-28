// EnviroMonitorApp/Services/Apis/MeasurementsResponse.cs
using Refit;
using System.Text.Json;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// Response wrapper for /v3/sensors/{id}/measurements
    /// </summary>
    public class MeasurementsResponse
    {
        public MeasurementsMeta Meta { get; set; } = null!;
        public Measurement[]    Results { get; set; } = null!;
    }

    public class MeasurementsMeta
    {
        public string Name    { get; set; } = null!;
        public string Website { get; set; } = null!;
        public int    Page    { get; set; }
        public int    Limit   { get; set; }

        // "found" can be a number or a string like ">100"
        [AliasAs("found")]
        public JsonElement Found { get; set; }
    }

    public class Measurement
    {
        public double    Value     { get; set; }
        public Parameter Parameter { get; set; } = null!;
        public PeriodWrapper Period { get; set; } = null!;
    }

    public class PeriodWrapper
    {
        public string       Label    { get; set; } = null!;
        public string       Interval { get; set; } = null!;

        [AliasAs("datetimeFrom")]
        public DateWrapper DatetimeFrom { get; set; } = null!;

        [AliasAs("datetimeTo")]
        public DateWrapper DatetimeTo   { get; set; } = null!;
    }
}
