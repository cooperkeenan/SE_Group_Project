// EnviroMonitorApp/Services/Apis/LocationLatestResponse.cs
using Refit;


namespace EnviroMonitorApp.Services.Apis
{
    public class LocationLatestResponse
    {
        public Meta Meta         { get; set; } = null!;
        public LatestResult[] Results { get; set; } = null!;
    }

    public class LatestResult
    {
        public DateWrapper Datetime  { get; set; } = null!;
        public double      Value     { get; set; }
        public int         SensorsId { get; set; }
        // you can ignore coords here—you're pivoting by parameter
    }
}
