// EnviroMonitorApp/Services/Apis/IWeatherApi.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace EnviroMonitorApp.Services.Apis
{
    /// <summary>
    /// POCOs matching the OpenWeather 5-day/3-hour forecast JSON
    /// </summary>
    public class OpenWeatherForecastResponse
    {
        // The API returns an array called "list"
        [AliasAs("list")]
        public List<ListItem> List { get; set; } = null!;

        public class ListItem
        {
            [AliasAs("dt")]
            public long Dt { get; set; }

            [AliasAs("main")]
            public MainInfo Main { get; set; } = null!;

            [AliasAs("wind")]
            public WindInfo Wind { get; set; } = null!;
        }

        public class MainInfo
        {
            [AliasAs("temp")]
            public double Temp { get; set; }

            [AliasAs("humidity")]
            public double Humidity { get; set; }
        }

        public class WindInfo
        {
            [AliasAs("speed")]
            public double Speed { get; set; }
        }
    }

    /// <summary>
    /// Refit interface for OpenWeather’s forecast endpoint
    /// </summary>
    public interface IWeatherApi
    {
        [Get("/data/2.5/forecast")]
        Task<OpenWeatherForecastResponse> GetForecast(
            [AliasAs("lat")]   double lat,
            [AliasAs("lon")]   double lon,
            [AliasAs("appid")] string apiKey,
            [AliasAs("units")] string units
        );
    }
}
