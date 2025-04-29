using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using SkiaSharp;                  // for SKColor
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public partial class AirQualityViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public AirQualityViewModel(IEnvironmentalDataService dataService)
        {
            _dataService    = dataService;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            ChartData = new ObservableCollection<ChartEntry>();

            // default sensor types
            SensorTypes = new[] { "NO₂", "SO₂", "PM₂.₅", "PM₁₀" };
            SelectedSensorType = SensorTypes.First();
        }

        // 1️⃣ Sensor picker support
        public string[] SensorTypes { get; }
        
        [ObservableProperty]
        string selectedSensorType;

        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
        {
            // reload chart when pollutant changes
            _ = LoadDataAsync();
        }

        [ObservableProperty]
        DateTime startDate = DateTime.UtcNow.AddDays(-7);
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        DateTime endDate   = DateTime.UtcNow;
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        string selectedRegion = string.Empty;
        partial void OnSelectedRegionChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        bool isBusy;

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand LoadDataCommand { get; }

        async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ChartData.Clear();

                var records = await _dataService
                    .GetAirQualityAsync(StartDate, EndDate, SelectedRegion);

                foreach (var rec in records)
                {
                    // 3️⃣ pick value based on selected sensor
                    double val = SelectedSensorType switch
                    {
                        "NO₂"  => rec.NO2,
                        "SO₂"  => rec.SO2,
                        "PM₂.₅" => rec.PM25,
                        "PM₁₀" => rec.PM10,
                        _      => 0
                    };

                    ChartData.Add(new ChartEntry((float)val)
                    {
                        Label      = rec.Timestamp.ToString("MM/dd HH:mm"),
                        ValueLabel = val.ToString("F1"),

                        // 4️⃣ give each a color so zeros don’t all blend
                        Color      = SKColor.Parse("#FF6200EE")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AirQuality load failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
