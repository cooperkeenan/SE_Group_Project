// ViewModels/WeatherViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public class WeatherViewModel : INotifyPropertyChanged
    {
        private readonly IEnvironmentalDataService _svc;

        public ObservableCollection<WeatherRecord> Forecast { get; } =
            new ObservableCollection<WeatherRecord>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public WeatherViewModel(IEnvironmentalDataService svc) => _svc = svc;

        public async Task LoadAsync()
        {
            var list = await _svc.GetWeatherAsync();
            Forecast.Clear();
            foreach (var r in list) Forecast.Add(r);
            OnPropertyChanged(nameof(Forecast));
        }

        void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
