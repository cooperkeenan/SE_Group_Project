using EnviroMonitor.Core.Services;

namespace EnviroMonitor.Core.Services
{
    /// <summary>
    /// High‑level service that works with environmental data
    /// (water quality, air quality, weather, etc.).  
    /// For now it just holds a reference to <see cref="ExcelReaderService"/>;
    /// real parsing logic will be added later.
    /// </summary>
    public class EnvironmentalDataService
    {
        /// <summary>Helper that loads Excel workbooks.</summary>
        private readonly ExcelReaderService _excel;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="excel">
        /// Instance of <see cref="ExcelReaderService"/> injected at startup.
        /// </param>
        public EnvironmentalDataService(ExcelReaderService excel)
        {
            _excel = excel;
        }

        /// <summary>
        /// Placeholder method used by unit tests;
        /// currently does nothing and should never throw.
        /// </summary>
        public void DoNothing() { }
    }
}
