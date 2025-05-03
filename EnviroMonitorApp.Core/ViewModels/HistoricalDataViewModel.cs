// EnviroMonitorApp/ViewModels/HistoricalDataViewModel.cs
using System;
using System.Collections.Generic;
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
using EnviroMonitorApp.Services.ChartTransformers;
using EnviroMonitorApp.Core.ViewModels;


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
            _dataService    = dataService;
            _transformer    = transformer;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            ChartData = new ObservableCollection<ChartEntry>();
            ChartData.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Chart));

            SensorTypes        = new[] { "Air", "Weather", "Water" };
            SelectedSensorType = SensorTypes.First();
            UpdateMetricTypes();

            StartDate = new DateTime(2010, 1, 1);
            EndDate   = DateTime.UtcNow.Date;
            UpdateMaxDate();
        }

        public string[] SensorTypes { get; }

        [ObservableProperty]
        private string selectedSensorType;
        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
        {
            UpdateMetricTypes();
            UpdateMaxDate();
            _ = LoadDataAsync();
        }

        [ObservableProperty]
        private string[] metricTypes;

        [ObservableProperty]
        private string selectedMetric;
        partial void OnSelectedMetricChanged(string oldValue, string newValue)
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
        [ObservableProperty] private DateTime maxDate;

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand                         LoadDataCommand { get; }

        public Chart Chart
            => new LineChart
            {
                Entries       = ChartData.Any()
                                  ? (IList<ChartEntry>)ChartData
                                  : new[] { new ChartEntry(0f) },
                LineSize      = 3,
                PointSize     = 6,
                LineAreaAlpha = 50,
                LabelTextSize = 14
            };

        void UpdateMetricTypes()
        {
            switch (SelectedSensorType)
            {
                case "Air":
                    MetricTypes = new[] { "NO₂", "PM₂.₅", "PM₁₀" };
                    break;
                case "Weather":
                    MetricTypes = new[]
                    {
                        "CloudCover", "Sunshine", "GlobalRadiation",
                        "MaxTemp", "MeanTemp", "MinTemp",
                        "Precipitation", "Pressure", "SnowDepth"
                    };
                    break;
                case "Water":
                    MetricTypes = new[] { "Nitrate", "PH", "DissolvedOxygen", "Temperature" };
                    break;
                default:
                    MetricTypes = Array.Empty<string>();
                    break;
            }
            SelectedMetric = MetricTypes.FirstOrDefault();
        }

        void UpdateMaxDate()
        {
            MaxDate = SelectedSensorType == "Weather"
                ? new DateTime(2020, 12, 31)
                : DateTime.UtcNow.Date;

            if (EndDate > MaxDate)   EndDate   = MaxDate;
            if (StartDate > MaxDate) StartDate = MaxDate;
        }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true; NoData = false;
            ChartData.Clear();

            Debug.WriteLine($"[HistoryVM] Loading {SelectedSensorType}.{SelectedMetric} from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

            IEnumerable<(DateTime timestamp, double value)> raw;
            string regionParam = "London";

            if (SelectedSensorType == "Air")
            {
                var air = await _dataService.GetAirQualityAsync(StartDate, EndDate, regionParam);
                raw = air.Select(r => (r.Timestamp,
                    SelectedMetric switch
                    {
                        "NO₂"   => r.NO2,
                        "PM₂.₅" => r.PM25,
                        "PM₁₀"  => r.PM10,
                        _       => 0
                    }));
            }
            else if (SelectedSensorType == "Weather")
            {
                var wx = await _dataService.GetWeatherAsync(StartDate, EndDate, regionParam);
                raw = wx.Select(r => (r.Timestamp,
                    SelectedMetric switch
                    {
                        "CloudCover"       => r.CloudCover,
                        "Sunshine"         => r.Sunshine,
                        "GlobalRadiation"  => r.GlobalRadiation,
                        "MaxTemp"          => r.MaxTemp,
                        "MeanTemp"         => r.MeanTemp,
                        "MinTemp"          => r.MinTemp,
                        "Precipitation"    => r.Precipitation,
                        "Pressure"         => r.Pressure,
                        "SnowDepth"        => r.SnowDepth,
                        _                  => 0
                    }));
            }
            else // Water
            {
                var wq = await _dataService.GetWaterQualityAsync(StartDate, EndDate, regionParam);
                raw = wq.Select(r => (r.Timestamp,
                    SelectedMetric switch
                    {
                        "Nitrate"          => r.Nitrate  ?? 0,
                        "PH"               => r.PH       ?? 0,
                        "DissolvedOxygen"  => r.DissolvedOxygen ?? 0,
                        "Temperature"      => r.Temperature     ?? 0,
                        _                  => 0
                    }));
            }

            var entries = _transformer.Transform(raw, StartDate, EndDate);
            if (entries.Count == 0)
            {
                NoData = true;
            }
            else
            {
                foreach (var e in entries) ChartData.Add(e);
            }

            IsBusy = false;
        }
    }
}
