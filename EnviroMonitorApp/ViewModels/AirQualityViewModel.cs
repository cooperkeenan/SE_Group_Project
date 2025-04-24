using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public class AirQualityViewModel : INotifyPropertyChanged
    {
        private readonly IEnvironmentalDataService _dataService;

        public ObservableCollection<AirQualityRecord> AirQuality { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AirQualityViewModel(IEnvironmentalDataService dataService)
        {
            _dataService = dataService;
            AirQuality = new ObservableCollection<AirQualityRecord>();
        }

        public async Task LoadAsync()
        {
            try
            {

                var data = await _dataService.GetAirQualityAsync();

                AirQuality.Clear();
                foreach (var record in data)
                {
                    AirQuality.Add(record);
                }

                OnPropertyChanged(nameof(AirQuality));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" Load failed: {ex}");
                throw;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
