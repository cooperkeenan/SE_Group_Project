// ViewModels/AirQualityViewModel.cs
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
    public partial class AirQualityViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public AirQualityViewModel(IEnvironmentalDataService dataService)
        {
            _dataService    = dataService ?? throw new ArgumentNullException(nameof(dataService));
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            ChartData = new ObservableCollection<ChartEntry>();

            // default sensor types
            SensorTypes        = new[] { "NO₂", "SO₂", "PM₂.₅", "PM₁₀" };
            SelectedSensorType = SensorTypes.First();

            // default region to London
            SelectedRegion = "London";
        }

        // 1️⃣ Sensor picker support
        public string[] SensorTypes { get; }

        [ObservableProperty]
        private string selectedSensorType;

        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private DateTime startDate = DateTime.UtcNow.AddDays(-7);
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private DateTime endDate   = DateTime.UtcNow;
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private string selectedRegion;
        partial void OnSelectedRegionChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private bool isBusy;

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand                          LoadDataCommand { get; }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy     = true;
                ChartData.Clear();

                // 2️⃣ Clamp to full days
                var fromDate = StartDate.Date;
                var toDate   = EndDate.Date;

                // 3️⃣ Default "All" or empty to London
                var regionParam = string.IsNullOrWhiteSpace(SelectedRegion) || SelectedRegion == "All"
                    ? "London"
                    : SelectedRegion;

                Debug.WriteLine($"[AirQualityVM] Querying DB from {fromDate:O} to {toDate:O} (region={regionParam})");

                var records = await _dataService
                    .GetAirQualityAsync(fromDate, toDate, regionParam);

                Debug.WriteLine($"[AirQualityVM] Retrieved {records?.Count ?? 0} records");

                if (records == null || records.Count == 0)
                    return;

                foreach (var rec in records)
                {
                    // pick the pollutant value
                    double val = SelectedSensorType switch
                    {
                        "NO₂"   => rec.NO2,
                        "SO₂"   => rec.SO2,
                        "PM₂.₅" => rec.PM25,
                        "PM₁₀"  => rec.PM10,
                        _       => 0
                    };

                    ChartData.Add(new ChartEntry((float)val)
                    {
                        Label      = rec.Timestamp.ToString("MM/dd HH:mm"),
                        ValueLabel = val.ToString("F1"),
                        Color      = SKColor.Parse("#FF6200EE")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ AirQualityVM.LoadDataAsync failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
