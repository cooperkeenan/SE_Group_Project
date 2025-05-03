using Refit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnviroMonitorApp.Services.Apis
{
    public class WaterQualityResponse
    {
        public List<ReadingItem> Items { get; set; } = new();
    }

    public class ReadingItem
    {
        public string DateTime { get; set; } = null!;
        public double Value    { get; set; }
    }

    public interface IWaterQualityApi
    {
        [Get("/hydrology/data/readings.json")]
        Task<WaterQualityResponse> GetLatest(
            [AliasAs("latest")]  bool   latest,
            [AliasAs("measure")] string measureUrl
        );

        // ⬇️ second overload – note the final parenthesis before the semicolon!
        [Get("/hydrology/data/readings.json")]
        Task<WaterQualityResponse> GetRange(
            [AliasAs("measure")] string measureUrl,
            [AliasAs("since")]   string sinceUtc,
            [AliasAs("_limit")]  int    limit = 96
        );  
    }
}
