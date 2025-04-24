using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public class WaterQualityViewModel
    {
        // ➊  Declare the field so we can call the service
        private readonly IEnvironmentalDataService _dataService;

        public ObservableCollection<WaterQualityRecord> WaterQuality { get; } = new();

        public WaterQualityViewModel(IEnvironmentalDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task LoadAsync()
        {
            var list = await _dataService.GetWaterQualityAsync();   

            WaterQuality.Clear();
            foreach (var item in list)
                WaterQuality.Add(item);
        }
    }
}
