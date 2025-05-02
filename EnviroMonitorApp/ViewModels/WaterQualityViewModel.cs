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
    public partial class WaterQualityViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _svc;

        public WaterQualityViewModel(IEnvironmentalDataService svc)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            // Setup data collections
            WaterQuality = new ObservableCollection<WaterQualityRecord>();
            ChartData     = new ObservableCollection<ChartEntry>();

            // Available measures
            SensorTypes         = new[] { "Nitrate", "PH", "Dissolved Oxygen", "Temperature" };
            SelectedSensorType  = SensorTypes.First();
        }

        // Raw data list for XAML bindings
        public ObservableCollection<WaterQualityRecord> WaterQuality { get; }

        // Chart entries for display
        public ObservableCollection<ChartEntry> ChartData { get; }

        public ICommand LoadDataCommand { get; }

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
        private DateTime endDate = DateTime.UtcNow;
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private string selectedRegion = string.Empty;
        partial void OnSelectedRegionChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private bool isBusy;

        // Chart property bound in XAML
        public Chart Chart => new LineChart
        {
            Entries       = ChartData.Any() ? ChartData.ToList() : new[] { new ChartEntry(0f) { Label = "", ValueLabel = "" } },
            LineSize      = 3,
            PointSize     = 6,
            LineAreaAlpha = 50,
            LabelTextSize = 14
        };

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                WaterQuality.Clear();
                ChartData.Clear();

                var regionParam = string.IsNullOrWhiteSpace(SelectedRegion) || SelectedRegion == "All"
                    ? string.Empty
                    : SelectedRegion;

                Debug.WriteLine($"\n[WaterVM] → Querying water {StartDate:O} → {EndDate:O} (region='{regionParam}')");

                var records = await _svc.GetWaterQualityAsync(StartDate, EndDate, regionParam);
                Debug.WriteLine($"[WaterVM] ← Retrieved {records?.Count ?? 0} records");

                if (records == null || !records.Any())
                    return;

                // dump out the first few records
                foreach (var rec in records.Take(5))
                {
                    Debug.WriteLine($"    {rec.Timestamp:yyyy-MM-dd HH:mm}   NO3={rec.Nitrate:F2}   pH={rec.PH:F2}   DO={rec.DissolvedOxygen:F2}   T={rec.Temperature:F2}");
                }

                // populate raw list
                foreach (var rec in records.OrderBy(r => r.Timestamp))
                    WaterQuality.Add(rec);

                // build chart
                foreach (var rec in WaterQuality)
                {
                    double? val = SelectedSensorType switch
                    {
                        "Nitrate"          => rec.Nitrate,
                        "PH"               => rec.PH,
                        "Dissolved Oxygen" => rec.DissolvedOxygen,
                        "Temperature"      => rec.Temperature,
                        _                  => null
                    };

                    float displayVal = (float)(val ?? 0);
                    ChartData.Add(new ChartEntry(displayVal)
                    {
                        Label      = rec.Timestamp.ToString("MM/dd HH:mm"),
                        ValueLabel = displayVal.ToString("F1"),
                        Color      = SKColor.Parse("#FF6200EE")
                    });
                }

                Debug.WriteLine($"[WaterVM] ChartData entries: {ChartData.Count}");
                if (ChartData.Any())
                    Debug.WriteLine($"    First point: {ChartData[0].Label} = {ChartData[0].ValueLabel}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ WaterVM.LoadDataAsync failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
