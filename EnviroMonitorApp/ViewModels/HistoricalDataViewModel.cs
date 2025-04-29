using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class HistoricalDataViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public HistoricalDataViewModel(IEnvironmentalDataService dataService)
        {
            _dataService    = dataService;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);

            ChartData = new ObservableCollection<ChartEntry>();
            RawData   = new ObservableCollection<AirQualityRecord>();

            // when the chart data collection changes, notify the Chart getter
            ChartData.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Chart));

            // set up the pickers
            SensorTypes        = new[] { "Air", "Weather", "Water" };
            Regions            = new[] { "All", "London" };
            SelectedSensorType = SensorTypes.First();
            SelectedRegion     = Regions.First();
            StartDate          = DateTime.UtcNow.AddDays(-7);
            EndDate            = DateTime.UtcNow;

            // listen for ANY of the four filter props changing
            PropertyChanged += Filter_PropertyChanged;
        }

        void Filter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(StartDate)
             or nameof(EndDate)
             or nameof(SelectedSensorType)
             or nameof(SelectedRegion))
            {
                // re‐load in the background
                _ = LoadDataAsync();
            }
        }

        // — your filters, fully observable —
        public string[] SensorTypes { get; }
        string _selectedSensorType;
        public  string  SelectedSensorType
        {
            get => _selectedSensorType;
            set => SetProperty(ref _selectedSensorType, value);
        }

        public string[] Regions { get; }
        string _selectedRegion;
        public  string  SelectedRegion
        {
            get => _selectedRegion;
            set => SetProperty(ref _selectedRegion, value);
        }

        DateTime _startDate;
        public  DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        DateTime _endDate;
        public  DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // — data & commands —
        public ObservableCollection<ChartEntry>       ChartData { get; }
        public ObservableCollection<AirQualityRecord> RawData   { get; }
        public ICommand                              LoadDataCommand { get; }

        /// <summary>
        /// Bound to your ChartView.Chart
        /// </summary>
        public Chart Chart
        {
            get
            {
                var entries = ChartData.Any()
                    ? (IList<ChartEntry>)ChartData
                    : new[]
                    {
                        new ChartEntry(0f)
                        {
                            Label      = "",
                            ValueLabel = ""
                        }
                    };

                return new LineChart
                {
                    Entries            = entries,
                    LineSize           = 3,
                    PointSize          = 6,
                    LineAreaAlpha      = 30,
                    LineMode           = LineMode.Straight,
                    LabelTextSize      = 14,
                    ValueLabelTextSize = 12
                };
            }
        }

        async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ChartData.Clear();
                RawData.Clear();

                // 1️⃣ fetch exactly the requested span
                var records = await _dataService
                    .GetAirQualityAsync(StartDate, EndDate, SelectedRegion);

                // 2️⃣ store raw
                foreach (var r in records)
                    RawData.Add(r);

                // 3️⃣ if >7 days, daily‐aggregate
                var days = (EndDate.Date - StartDate.Date).TotalDays;
                bool daily = days > 7;

                var plotSet = daily
                    ? records
                        .GroupBy(r => r.Timestamp.Date)
                        .Select(g => new AirQualityRecord {
                            Timestamp = g.Key,
                            NO2       = g.Average(x => x.NO2),
                            SO2       = g.Average(x => x.SO2),
                            PM25      = g.Average(x => x.PM25),
                            PM10      = g.Average(x => x.PM10)
                        })
                        .OrderBy(r => r.Timestamp)
                        .ToList()
                    : records.OrderBy(r => r.Timestamp).ToList();

                // 4️⃣ build chart entries
                foreach (var rec in plotSet)
                {
                    var val = SelectedSensorType switch
                    {
                        "Air"     => rec.NO2,
                        "Weather" => 0,
                        "Water"   => 0,
                        _         => rec.NO2
                    };

                    ChartData.Add(new ChartEntry((float)val)
                    {
                        Label      = daily
                                     ? rec.Timestamp.ToString("MM/dd")
                                     : rec.Timestamp.ToString("MM/dd HH:mm"),
                        ValueLabel = daily ? "" : val.ToString("F1"),
                        Color      = SKColor.Parse("#FF6200EE")
                    });
                }

                Debug.WriteLine($"⚙️ ChartData loaded {ChartData.Count} points");
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
