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


namespace EnviroMonitorApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Historical Data page, providing data and commands
    /// for visualizing historical environmental measurements.
    /// </summary>
    public partial class HistoricalDataViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;
        readonly IChartTransformer         _transformer;

        /// <summary>
        /// Initializes a new instance of the HistoricalDataViewModel class.
        /// </summary>
        /// <param name="dataService">Service for retrieving environmental data.</param>
        /// <param name="transformer">Transformer for converting data into chart entries.</param>
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

        /// <summary>
        /// Gets the available sensor types (Air, Weather, Water).
        /// </summary>
        public string[] SensorTypes { get; }

        /// <summary>
        /// Gets or sets the currently selected sensor type.
        /// </summary>
        [ObservableProperty]
        private string selectedSensorType;
        
        /// <summary>
        /// Handles changes to the selected sensor type by updating metrics and reloading data.
        /// </summary>
        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
        {
            UpdateMetricTypes();
            UpdateMaxDate();
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Gets or sets the available metric types for the current sensor type.
        /// </summary>
        [ObservableProperty]
        private string[] metricTypes;

        /// <summary>
        /// Gets or sets the currently selected metric.
        /// </summary>
        [ObservableProperty]
        private string selectedMetric;
        
        /// <summary>
        /// Handles changes to the selected metric by reloading data.
        /// </summary>
        partial void OnSelectedMetricChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        /// <summary>
        /// Gets or sets the start date for data retrieval.
        /// </summary>
        [ObservableProperty]
        private DateTime startDate;
        
        /// <summary>
        /// Handles changes to the start date by reloading data.
        /// </summary>
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        /// <summary>
        /// Gets or sets the end date for data retrieval.
        /// </summary>
        [ObservableProperty]
        private DateTime endDate;
        
        /// <summary>
        /// Handles changes to the end date by reloading data.
        /// </summary>
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue)
            => _ = LoadDataAsync();

        /// <summary>
        /// Gets or sets a value indicating whether data is currently being loaded.
        /// </summary>
        [ObservableProperty] private bool isBusy;
        
        /// <summary>
        /// Gets or sets a value indicating whether no data is available for the current selection.
        /// </summary>
        [ObservableProperty] private bool noData;
        
        /// <summary>
        /// Gets or sets the maximum allowed end date based on the selected sensor type.
        /// </summary>
        [ObservableProperty] private DateTime maxDate;

        /// <summary>
        /// Gets the collection of chart entries for visualization.
        /// </summary>
        public ObservableCollection<ChartEntry> ChartData { get; }
        
        /// <summary>
        /// Gets the command to load data based on current selections.
        /// </summary>
        public ICommand LoadDataCommand { get; }

        /// <summary>
        /// Gets a chart object configured with the current chart data.
        /// </summary>
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

        /// <summary>
        /// Updates the available metric types based on the currently selected sensor type.
        /// </summary>
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

        /// <summary>
        /// Updates the maximum allowed end date based on the currently selected sensor type.
        /// </summary>
        void UpdateMaxDate()
        {
            MaxDate = SelectedSensorType == "Weather"
                ? new DateTime(2020, 12, 31)
                : DateTime.UtcNow.Date;

            if (EndDate > MaxDate)   EndDate   = MaxDate;
            if (StartDate > MaxDate) StartDate = MaxDate;
        }

        /// <summary>
        /// Asynchronously loads data based on the current selections and updates the chart.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true; NoData = false;
            ChartData.Clear();

            Debug.WriteLine($"[HistoryVM] Loading {SelectedSensorType}.{SelectedMetric} from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

            // Change raw to use nullable double (double?)
            IEnumerable<(DateTime timestamp, double?)> raw;
            string regionParam = "London";

            if (SelectedSensorType == "Air")
            {
                var air = await _dataService.GetAirQualityAsync(StartDate, EndDate, regionParam);
                raw = air.Select(r => (r.Timestamp,
                    SelectedMetric switch
                    {
                        "NO₂"   => r.NO2 as double?,    // Explicitly cast to nullable double
                        "PM₂.₅" => r.PM25 as double?,   
                        "PM₁₀"  => r.PM10 as double?,  
                        _       => null
                    }));
            }
            else if (SelectedSensorType == "Weather")
            {
                var wx = await _dataService.GetWeatherAsync(StartDate, EndDate, regionParam);
                raw = wx.Select(r => (r.Timestamp,
                    SelectedMetric switch
                    {
                        "CloudCover"       => r.CloudCover as double?,  // Explicitly cast to nullable double
                        "Sunshine"         => r.Sunshine as double?,
                        "GlobalRadiation"  => r.GlobalRadiation as double?,
                        "MaxTemp"          => r.MaxTemp as double?,
                        "MeanTemp"         => r.MeanTemp as double?,
                        "MinTemp"          => r.MinTemp as double?,
                        "Precipitation"    => r.Precipitation as double?,
                        "Pressure"         => r.Pressure as double?,
                        "SnowDepth"        => r.SnowDepth as double?,
                        _                  => null
                    }));
            }
            else // Water
            {
                var wq = await _dataService.GetWaterQualityAsync(StartDate, EndDate, regionParam);
                raw = wq.Select(r => (r.Timestamp,
                    SelectedMetric switch
                    {
                        "Nitrate"          => r.Nitrate as double?,  // Explicitly cast to nullable double
                        "PH"               => r.PH as double?,
                        "DissolvedOxygen"  => r.DissolvedOxygen as double?,
                        "Temperature"      => r.Temperature as double?,
                        _                  => null
                    }));
            }

            // Transform the data using the chart transformer
            var clean   = raw.Where(t => t.Item2.HasValue).Select(t => (t.timestamp, t.Item2!.Value));
            var entries = _transformer.Transform(clean, StartDate, EndDate);
            // Check if entries are empty
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