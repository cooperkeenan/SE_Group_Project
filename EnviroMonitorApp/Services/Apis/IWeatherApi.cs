// Services/Apis/IWeatherApi.cs
using Refit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnviroMonitorApp.Services.Apis
{
    public class OpenWeatherForecastResponse
    {
        public ListItem[] List { get; set; } = null!;
        public class ListItem
        {
            public long Dt { get; set; }
            public MainInfo Main { get; set; } = null!;
            public WindInfo Wind { get; set; } = null!;
        }
        public class MainInfo
        {
            public double Temp { get; set; }
            public double Humidity { get; set; }
        }
        public class WindInfo
        {
            public double Speed { get; set; }
        }
    }

    public interface IWeatherApi
    {
        // forecast endpoint returns a List<...>
        [Get("/data/2.5/forecast")] 
        Task<OpenWeatherForecastResponse> GetForecast(
            [AliasAs("lat")] double lat,
            [AliasAs("lon")] double lon,
            [AliasAs("appid")] string apiKey,
            [AliasAs("units")] string units);
    }
}
