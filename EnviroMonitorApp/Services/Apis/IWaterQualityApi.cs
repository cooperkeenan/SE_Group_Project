using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services.Apis
{
    public class WaterQualityResponse
    {
        public WaterResult[] Results { get; set; } = null!;
    }

    public class WaterResult
    {
        public DateTime ActivityStartDateTime { get; set; }
        public string CharacteristicName { get; set; } = null!;
        public double Value { get; set; }
    }

    public interface IWaterQualityApi
    {
        // note: base address ends in /iv/
        [Get("")]
        Task<WaterQualityResponse> Search(
            [AliasAs("format")] string format,
            [AliasAs("startDT")] string startDate,               // <-- parameter name is "startDate"
            [AliasAs("characteristicName")] string characteristicName
        );
    }
}
