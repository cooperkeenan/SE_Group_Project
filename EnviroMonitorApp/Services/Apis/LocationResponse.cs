// EnviroMonitorApp/Services/Apis/LocationResponse.cs
using Refit;
using System.Text.Json; 


namespace EnviroMonitorApp.Services.Apis
{
    public class LocationResponse
    {
        public Meta    Meta    { get; set; } = null!;
        public Location[] Results { get; set; } = null!;
    }

    public class Meta
    {
        public int    Page  { get; set; }
        public int    Limit { get; set; }
        public JsonElement Found { get; set; } 
    }

    public class Location
    {
        public int      Id        { get; set; }
        public string   Name      { get; set; } = null!;
        public Sensor[] Sensors   { get; set; } = null!;
        public DateWrapper DatetimeLast { get; set; } = null!;  // last measurement time
    }

    public class Sensor
    {
        public int       Id        { get; set; }
        public string    Name      { get; set; } = null!;
        public Parameter Parameter { get; set; } = null!;
    }

    public class Parameter
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
    }

    public class DateWrapper
    {
        [AliasAs("utc")]
        public string Utc         { get; set; } = null!;
        [AliasAs("local")]
        public string Local       { get; set; } = null!;
    }
}
