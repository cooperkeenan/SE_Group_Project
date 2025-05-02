// ViewModels/HistoricalDataViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
    public partial class HistoricalDataViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public HistoricalDataViewModel(IEnvironmentalDataService dataService)
        {
            _dataService    = dataService ?? throw new ArgumentNullException(nameof(dataService));
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            ChartData = new ObservableCollection<ChartEntry>();
            ChartData.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Chart));

            // Only Air for now
            SensorTypes        = new[] { "Air" };
            Regions            = new[] { "All", "London" };
            SelectedSensorType = "Air";
            SelectedRegion     = "All";
        }

        public string[] SensorTypes { get; }
        public string[] Regions     { get; }

        [ObservableProperty]
        private string selectedSensorType;
        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private string selectedRegion;
        partial void OnSelectedRegionChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private DateTime startDate = DateTime.UtcNow.AddDays(-7);
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private DateTime endDate = DateTime.UtcNow;
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool noData;

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand                         LoadDataCommand { get; }

        public Chart Chart
        {
            get
            {
                var entries = ChartData.Any()
                    ? (IList<ChartEntry>)ChartData.ToList()
                    : new List<ChartEntry> { new ChartEntry(0f) { Label = "", ValueLabel = "" } };

                return new LineChart
                {
                    Entries       = entries,
                    LineSize      = 3,
                    PointSize     = 6,
                    LineAreaAlpha = 50,
                    LabelTextSize = 14
                };
            }
        }

        private async Task LoadDataAsync()
        {
            Debug.WriteLine($"\n[HistoryVM] >>> Loading AIR data {StartDate:yyyy-MM-dd} → {EndDate:yyyy-MM-dd}, region={SelectedRegion}");
            if (IsBusy)
            {
                Debug.WriteLine("[HistoryVM]   → Busy, skipping.");
                return;
            }

            try
            {
                IsBusy   = true;
                NoData   = false;
                ChartData.Clear();
                Debug.WriteLine("[HistoryVM]   → Cleared ChartData.");

                // Always query London when “All” or blank
                var regionParam = string.IsNullOrWhiteSpace(SelectedRegion) || SelectedRegion == "All"
                    ? "London"
                    : SelectedRegion;
                Debug.WriteLine($"[HistoryVM]   → regionParam = '{regionParam}'");

                // fetch air data
                var records = await _dataService.GetAirQualityAsync(StartDate.Date, EndDate.Date, regionParam);
                Debug.WriteLine($"[HistoryVM]   ← Retrieved {records?.Count ?? 0} air records");

                if (records == null || !records.Any())
                {
                    Debug.WriteLine("[HistoryVM]   !!! No air data returned");
                    NoData = true;
                    return;
                }

                // group by day if span >7, else hourly
                var spanDays = (EndDate.Date - StartDate.Date).TotalDays;
                var plotSet = spanDays > 7
                    ? records
                        .GroupBy(r => r.Timestamp.Date)
                        .Select(g => new AirQualityRecord {
                            Timestamp = g.Key,
                            NO2       = g.Average(x => x.NO2),
                            SO2       = g.Average(x => x.SO2),
                            PM25      = g.Average(x => x.PM25),
                            PM10      = g.Average(x => x.PM10)
                        })
                        .OrderBy(x => x.Timestamp)
                        .ToList()
                    : records.OrderBy(r => r.Timestamp).ToList();

                Debug.WriteLine($"[HistoryVM]   → Plotting {plotSet.Count} points");
                foreach (var rec in plotSet)
                {
                    var val = rec.NO2;
                    var label = spanDays > 7
                        ? rec.Timestamp.ToString("MM/dd", CultureInfo.InvariantCulture)
                        : rec.Timestamp.ToString("MM/dd HH:mm", CultureInfo.InvariantCulture);

                    Debug.WriteLine($"      • {label} → {val:F1}");
                    ChartData.Add(new ChartEntry((float)val)
                    {
                        Label      = label,
                        ValueLabel = val.ToString("F1", CultureInfo.InvariantCulture),
                        Color      = SKColor.Parse("#FF6200EE")
                    });
                }

                Debug.WriteLine($"[HistoryVM]   → ChartData.Count = {ChartData.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [HistoryVM] Error loading air: {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[HistoryVM] <<< Done loading air");
            }
        }
    }
}
