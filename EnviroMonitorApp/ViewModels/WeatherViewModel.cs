// ViewModels/WeatherViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public partial class WeatherViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public WeatherViewModel(IEnvironmentalDataService dataService)
        {
            _dataService    = dataService ?? throw new ArgumentNullException(nameof(dataService));
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            ChartData = new ObservableCollection<ChartEntry>();
            SensorTypes = new[] { "Temperature", "Humidity", "WindSpeed" };
            SelectedSensorType = SensorTypes.First();
        }

        public string[] SensorTypes { get; }
        [ObservableProperty] private string selectedSensorType;

        [ObservableProperty] private DateTime startDate = DateTime.UtcNow.AddDays(-7);
        [ObservableProperty] private DateTime endDate   = DateTime.UtcNow;

        [ObservableProperty] private bool isBusy;

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand                         LoadDataCommand { get; }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ChartData.Clear();

            Debug.WriteLine($"[WeatherVM] Fetching {SelectedSensorType} from {StartDate:O} to {EndDate:O}");

            var records = await _dataService.GetWeatherAsync(StartDate, EndDate, "");
            if (records == null || !records.Any())
            {
                IsBusy = false;
                return;
            }

            foreach (var rec in records.OrderBy(r => r.Timestamp))
            {
                double val = SelectedSensorType switch
                {
                    "Temperature" => rec.Temperature,
                    "Humidity"    => rec.Humidity,
                    "WindSpeed"   => rec.WindSpeed,
                    _             => 0
                };

                ChartData.Add(new ChartEntry((float)val)
                {
                    Label      = rec.Timestamp.ToString("MM/dd HH:mm"),
                    ValueLabel = val.ToString("F1"),
                    Color      = SKColor.Parse("#FF6200EE")
                });
            }

            IsBusy = false;
        }
    }
}
