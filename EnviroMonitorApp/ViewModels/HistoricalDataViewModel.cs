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
            _dataService       = dataService;
            LoadDataCommand    = new AsyncRelayCommand(LoadDataAsync);

            ChartData          = new ObservableCollection<ChartEntry>();
            RawData            = new ObservableCollection<AirQualityRecord>();

            // re-raise Chart whenever the entries collection changes:
            ChartData.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Chart));

            SensorTypes        = new[] { "Air", "Weather", "Water" };
            Regions            = new[] { "All", "London" };
            SelectedSensorType = SensorTypes.First();
            SelectedRegion     = Regions.First();
        }

        public string[] SensorTypes { get; }

        [ObservableProperty]
        private string selectedSensorType;
        partial void OnSelectedSensorTypeChanged(string oldValue, string newValue)
            => _ = LoadDataAsync();

        public string[] Regions { get; }

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

        public ObservableCollection<ChartEntry>       ChartData { get; }
        public ObservableCollection<AirQualityRecord> RawData   { get; }
        public ICommand                              LoadDataCommand { get; }

        public Chart Chart
        {
            get
            {
                var entries = ChartData.Any()
                    ? (IList<ChartEntry>)ChartData
                    : new[] { new ChartEntry(0f) { Label = "", ValueLabel = "" } };

                return new LineChart
                {
                    Entries       = entries.ToList(),
                    LineSize      = 3,
                    PointSize     = 6,
                    LineAreaAlpha = 50,
                    LabelTextSize = 14
                };
            }
        }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy   = true;
                NoData   = false;
                ChartData.Clear();
                RawData.Clear();

                var regionParam = SelectedRegion == "All"
                    ? "London"
                    : SelectedRegion;

                var records = await _dataService
                    .GetAirQualityAsync(StartDate, EndDate, regionParam);

                if (records == null || records.Count == 0)
                {
                    NoData = true;
                    return;
                }

                foreach (var r in records)
                    RawData.Add(r);

                var spanDays = (EndDate.Date - StartDate.Date).TotalDays;
                var plotSet  = spanDays > 7
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
                    : records.OrderBy(x => x.Timestamp).ToList();

                foreach (var rec in plotSet)
                {
                    var val = SelectedSensorType switch
                    {
                        "Air"     => rec.NO2,
                        "Weather" => 0,
                        "Water"   => 0,
                        _         => rec.NO2
                    };

                    var label = spanDays > 7
                        ? rec.Timestamp.ToString("MM/dd", CultureInfo.InvariantCulture)
                        : rec.Timestamp.ToString("MM/dd HH:mm", CultureInfo.InvariantCulture);

                    ChartData.Add(new ChartEntry((float)val)
                    {
                        Label      = label,
                        ValueLabel = val.ToString("F1", CultureInfo.InvariantCulture),
                        Color      = SKColor.Parse("#FF6200EE")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadDataAsync failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
