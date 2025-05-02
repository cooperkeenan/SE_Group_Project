// ViewModels/HistoricalDataViewModel.cs
using System;
using System.Collections.Generic;
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

            // Support Air and Water for now
            SensorTypes        = new[] { "Air", "Water" };
            Regions            = new[] { "All", "London" };
            SelectedSensorType = SensorTypes.First();
            SelectedRegion     = Regions.First();
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
                IList<ChartEntry> entries = ChartData.Any()
                    ? ChartData.ToList()
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
            Debug.WriteLine($"\n[HistoryVM] >>> Enter LoadDataAsync (Sensor={SelectedSensorType}, Region={SelectedRegion}, Range={StartDate:yyyy-MM-dd}→{EndDate:yyyy-MM-dd})");
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

                var regionParam = SelectedRegion == "All" || string.IsNullOrWhiteSpace(SelectedRegion)
                    ? "London"
                    : SelectedRegion;
                Debug.WriteLine($"[HistoryVM]   → Effective regionParam = '{regionParam}'");

                switch (SelectedSensorType)
                {
                    case "Air":
                        Debug.WriteLine("[HistoryVM]   → Branch: AIR");
                        var air = await _dataService.GetAirQualityAsync(StartDate.Date, EndDate.Date, regionParam);
                        Debug.WriteLine($"[HistoryVM]   ← Air records count = {air?.Count ?? 0}");

                        if (air == null || air.Count == 0)
                        {
                            Debug.WriteLine("[HistoryVM]   !!! No air data");
                            NoData = true;
                            return;
                        }

                        var spanDays = (EndDate.Date - StartDate.Date).TotalDays;
                        var airSet = spanDays > 7
                            ? air.GroupBy(r => r.Timestamp.Date)
                                 .Select(g => new AirQualityRecord
                                 {
                                     Timestamp = g.Key,
                                     NO2       = g.Average(x => x.NO2),
                                     SO2       = g.Average(x => x.SO2),
                                     PM25      = g.Average(x => x.PM25),
                                     PM10      = g.Average(x => x.PM10)
                                 })
                                 .OrderBy(r => r.Timestamp)
                                 .ToList()
                            : air.OrderBy(r => r.Timestamp).ToList();

                        Debug.WriteLine($"[HistoryVM]   → Plotting {airSet.Count} air points");
                        foreach (var rec in airSet)
                        {
                            var val = rec.NO2;
                            var label = spanDays > 7
                                ? rec.Timestamp.ToString("MM/dd", CultureInfo.InvariantCulture)
                                : rec.Timestamp.ToString("MM/dd HH:mm", CultureInfo.InvariantCulture);

                            Debug.WriteLine($"      • {label} = {val:F1}");
                            ChartData.Add(new ChartEntry((float)val)
                            {
                                Label      = label,
                                ValueLabel = val.ToString("F1", CultureInfo.InvariantCulture),
                                Color      = SKColor.Parse("#FF6200EE")
                            });
                        }
                        break;

                    case "Water":
                        Debug.WriteLine("[HistoryVM]   → Branch: WATER");
                        var water = await _dataService.GetWaterQualityAsync(StartDate.Date, EndDate.Date, regionParam);
                        Debug.WriteLine($"[HistoryVM]   ← Water records count = {water?.Count ?? 0}");

                        if (water == null || water.Count == 0)
                        {
                            Debug.WriteLine("[HistoryVM]   !!! No water data");
                            NoData = true;
                            return;
                        }

                        var waterSet = water.OrderBy(r => r.Timestamp).ToList();
                        Debug.WriteLine($"[HistoryVM]   → Plotting {waterSet.Count} water points");
                        foreach (var rec in waterSet)
                        {
                            var val = rec.Nitrate ?? 0;
                            var label = rec.Timestamp.ToString("MM/dd HH:mm", CultureInfo.InvariantCulture);

                            Debug.WriteLine($"      • {label} = {val:F1}");
                            ChartData.Add(new ChartEntry((float)val)
                            {
                                Label      = label,
                                ValueLabel = val.ToString("F1", CultureInfo.InvariantCulture),
                                Color      = SKColor.Parse("#FF6200EE")
                            });
                        }
                        break;

                    default:
                        Debug.WriteLine($"[HistoryVM]   !!! Unhandled sensor '{SelectedSensorType}'");
                        NoData = true;
                        return;
                }

                Debug.WriteLine($"[HistoryVM]   → Final ChartData.Count = {ChartData.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [HistoryVM] Error in LoadDataAsync: {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[HistoryVM] <<< Exit LoadDataAsync");
            }
        }
    }
}