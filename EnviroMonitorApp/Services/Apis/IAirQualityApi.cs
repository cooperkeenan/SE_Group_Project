using Refit;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services.Apis
{
    public class OpenAqResponse
    {
        public Measurement[] Results { get; set; } = null!;
        public class Measurement
        {
            public string Parameter { get; set; } = null!;
            public ValueWrapper Date { get; set; } = null!;
            public double Value { get; set; }
        }
        public class ValueWrapper
        {
            public DateWrapper Utc { get; set; } = null!;
        }
        public class DateWrapper
        {
            [AliasAs("utc")]
            public string Utc { get; set; } = null!;
        }
    }

    public interface IAirQualityApi
    {
        // v2/measurements is the current OpenAQ path
        [Get("/v2/measurements")]
        Task<OpenAqResponse> GetMeasurements(
            [AliasAs("city")] string city,
            [AliasAs("parameter")] string parameters,
            [AliasAs("date_from")] string fromUtc,
            [AliasAs("limit")] int limit);
    }
}
