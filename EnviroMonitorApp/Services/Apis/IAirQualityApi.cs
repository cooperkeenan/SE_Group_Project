// Services/Apis/IAirQualityApi.cs
using Refit;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services.Apis
{
    public interface IAirQualityApi
    {
        [Get("/v3/locations")]
        Task<LocationResponse> GetLocations(
            [AliasAs("iso")]           string iso,
            [AliasAs("coordinates")]   string latlon,
            [AliasAs("radius")]        int    radiusMeters,
            [AliasAs("parameters_id")] string parameterIdsCsv,
            [AliasAs("limit")]         int    limit
        );

        [Get("/v3/locations/{locationId}/latest")]
        Task<LocationLatestResponse> GetLocationLatest(int locationId);
    }
}
