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

            SensorTypes        = new[] { "Air", "Weather", "Water" };
            Regions            = new[] { "All", "London" };
            SelectedSensorType = SensorTypes.First();
            SelectedRegion     = Regions.First();

            StartDate = new DateTime(2010, 1, 1);
            EndDate   = DateTime.UtcNow.Date;
            UpdateMaxDate();
        }

        public string[] SensorTypes { get; }
        public string[] Regions     { get; }

        [ObservableProperty]
        private string selectedSensorType;
        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
        {
            UpdateMaxDate();
            _ = LoadDataAsync();
        }

        [ObservableProperty]
        private string selectedRegion;
        partial void OnSelectedRegionChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private DateTime startDate;
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty]
        private DateTime endDate;
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool noData;

        [ObservableProperty]
        private DateTime maxDate;

        private void UpdateMaxDate()
        {
            if (SelectedSensorType == "Weather")
                MaxDate = new DateTime(2020, 12, 31);
            else
                MaxDate = DateTime.UtcNow.Date;

            if (EndDate > MaxDate)   EndDate   = MaxDate;
            if (StartDate > MaxDate) StartDate = MaxDate;
        }

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand                         LoadDataCommand { get; }

        public Chart Chart
        {
            get
            {
                // force both branches to IList<ChartEntry>
                IList<ChartEntry> entries = ChartData.Any()
                    ? (IList<ChartEntry>)ChartData
                    : new List<ChartEntry>
                    {
                        new ChartEntry(0f)
                        {
                            Label      = "",
                            ValueLabel = ""
                        }
                    };

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
                IsBusy = true;
                NoData = false;
                ChartData.Clear();
                Debug.WriteLine("[HistoryVM]   → Cleared ChartData.");

                var regionParam = SelectedRegion == "All" ? "London" : SelectedRegion;
                Debug.WriteLine($"[HistoryVM]   → Effective regionParam = '{regionParam}'");

                switch (SelectedSensorType)
                {
                    case "Air":
                        Debug.WriteLine("[HistoryVM]   → Branch: AIR");
                        var air = await _dataService.GetAirQualityAsync(StartDate, EndDate, regionParam);
                        Debug.WriteLine($"[HistoryVM]   ← Air records count = {air?.Count ?? -1}");
                        if (air == null || !air.Any()) { NoData = true; return; }

                        var airSpan = (EndDate.Date - StartDate.Date).TotalDays;
                        var airSet = airSpan > 7
                            ? air.GroupBy(r => r.Timestamp.Date)
                                  .Select(g => new AirQualityRecord {
                                      Timestamp = g.Key,
                                      NO2       = g.Average(x => x.NO2),
                                      SO2       = g.Average(x => x.SO2),
                                      PM25      = g.Average(x => x.PM25),
                                      PM10      = g.Average(x => x.PM10)
                                  })
                                  .OrderBy(x => x.Timestamp)
                                  .ToList()
                            : air.OrderBy(r => r.Timestamp).ToList();

                        foreach (var rec in airSet)
                        {
                            var val   = rec.NO2;
                            var label = airSpan > 7
                                ? rec.Timestamp.ToString("MM/dd", CultureInfo.InvariantCulture)
                                : rec.Timestamp.ToString("MM/dd HH:mm", CultureInfo.InvariantCulture);

                            ChartData.Add(new ChartEntry((float)val)
                            {
                                Label      = label,
                                ValueLabel = val.ToString("F1", CultureInfo.InvariantCulture),
                                Color      = SKColor.Parse("#FF6200EE")
                            });
                        }
                        break;

                    case "Weather":
                        Debug.WriteLine("[HistoryVM]   → Branch: WEATHER (Climate min temp)");
                        var wx = await _dataService.GetWeatherAsync(StartDate, EndDate, regionParam);
                        Debug.WriteLine($"[HistoryVM]   ← Weather records count = {wx?.Count ?? -1}");
                        if (wx == null || !wx.Any()) { NoData = true; return; }

                        foreach (var rec in wx)
                        {
                            var val   = rec.Temperature;
                            var label = rec.Timestamp.ToString("MM/dd", CultureInfo.InvariantCulture);

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
                        var water = await _dataService.GetWaterQualityAsync(StartDate, EndDate, regionParam);
                        Debug.WriteLine($"[HistoryVM]   ← Water records count = {water?.Count ?? -1}");
                        if (water == null || !water.Any()) { NoData = true; return; }

                        foreach (var rec in water.OrderBy(r => r.Timestamp))
                        {
                            var val   = rec.Nitrate ?? 0;
                            var label = rec.Timestamp.ToString("MM/dd", CultureInfo.InvariantCulture);

                            ChartData.Add(new ChartEntry((float)val)
                            {
                                Label      = label,
                                ValueLabel = val.ToString("F1", CultureInfo.InvariantCulture),
                                Color      = SKColor.Parse("#FF6200EE")
                            });
                        }
                        break;

                    default:
                        Debug.WriteLine($"[HistoryVM]   !!! Unknown sensor type '{SelectedSensorType}'");
                        NoData = true;
                        return;
                }

                Debug.WriteLine($"[HistoryVM]   → Final ChartData.Count = {ChartData.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [HistoryVM] Exception: {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("[HistoryVM] <<< Exit LoadDataAsync");
            }
        }
    }
}
