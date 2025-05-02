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
using EnviroMonitorApp.Services.ChartTransformers;

namespace EnviroMonitorApp.ViewModels
{
    public partial class HistoricalDataViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;
        readonly IChartTransformer         _transformer;

        public HistoricalDataViewModel(
            IEnvironmentalDataService dataService,
            IChartTransformer         transformer)
        {
            _dataService    = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _transformer    = transformer ?? throw new ArgumentNullException(nameof(transformer));
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            // initialize collections
            ChartData = new ObservableCollection<ChartEntry>();
            ChartData.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Chart));

            // sensor/region pickers
            SensorTypes        = new[] { "Air", "Weather", "Water" };
            Regions            = new[] { "All", "London" };
            SelectedSensorType = SensorTypes.First();
            SelectedRegion     = Regions.First();

            // default date range
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

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand                         LoadDataCommand { get; }

        public Chart Chart
        {
            get
            {
                // always present an IList<ChartEntry>
                IList<ChartEntry> entries = ChartData.Any()
                    ? (IList<ChartEntry>)ChartData
                    : new List<ChartEntry> {
                        new ChartEntry(0f) {
                            Label      = "",
                            ValueLabel = ""
                        }
                      };

                return new LineChart {
                    Entries       = entries,
                    LineSize      = 3,
                    PointSize     = 6,
                    LineAreaAlpha = 50,
                    LabelTextSize = 14
                };
            }
        }

        void UpdateMaxDate()
        {
            // weather stops at end of 2020
            MaxDate = SelectedSensorType == "Weather"
                ? new DateTime(2020, 12, 31)
                : DateTime.UtcNow.Date;

            // clamp current selections
            if (EndDate > MaxDate)   EndDate   = MaxDate;
            if (StartDate > MaxDate) StartDate = MaxDate;
        }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            NoData = false;
            ChartData.Clear();

            Debug.WriteLine($"\n[HistoryVM] >>> LoadDataAsync(sensor={SelectedSensorType}, region={SelectedRegion}, from={StartDate:yyyy-MM-dd}, to={EndDate:yyyy-MM-dd})");

            // fetch raw (timestamp, value) pairs
            IEnumerable<(DateTime ts, double val)> raw;
            var regionParam = SelectedRegion == "All" ? "London" : SelectedRegion;

            switch (SelectedSensorType)
            {
                case "Air":
                    Debug.WriteLine("[HistoryVM]   → Fetching AirQuality");
                    var air = await _dataService.GetAirQualityAsync(StartDate, EndDate, regionParam);
                    raw = air.Select(r => (r.Timestamp, r.NO2));
                    break;

                case "Weather":
                    Debug.WriteLine("[HistoryVM]   → Fetching Weather");
                    var wx = await _dataService.GetWeatherAsync(StartDate, EndDate, regionParam);
                    raw = wx.Select(r => (r.Timestamp, r.Temperature));
                    break;

                case "Water":
                    Debug.WriteLine("[HistoryVM]   → Fetching WaterQuality");
                    var wq = await _dataService.GetWaterQualityAsync(StartDate, EndDate, regionParam);
                    raw = wq.Select(r => (r.Timestamp, r.Nitrate ?? 0));
                    break;

                default:
                    Debug.WriteLine("[HistoryVM]   !!! Unknown sensor");
                    IsBusy = false;
                    return;
            }

            var entries = _transformer.Transform(raw, StartDate, EndDate);
            if (entries == null || entries.Count == 0)
            {
                Debug.WriteLine("[HistoryVM]   !!! No data after transform");
                NoData = true;
            }
            else
            {
                Debug.WriteLine($"[HistoryVM]   → Plotting {entries.Count} points");
                foreach (var e in entries)
                    ChartData.Add(e);
            }

            IsBusy = false;
            Debug.WriteLine("[HistoryVM] <<< LoadDataAsync complete");
        }
    }
}
