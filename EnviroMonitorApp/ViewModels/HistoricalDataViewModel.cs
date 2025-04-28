using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using System.Collections.Generic;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public partial class HistoricalDataViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public HistoricalDataViewModel(IEnvironmentalDataService dataService)
        {
            Debug.WriteLine("⚙️ HistoricalDataViewModel: ctor");
            _dataService    = dataService;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            ChartData       = new ObservableCollection<ChartEntry>();
            RawData         = new ObservableCollection<AirQualityRecord>();
        }

        // ————————————————————————————————————— Filters & State
        private DateTime _startDate = DateTime.UtcNow.AddDays(-7);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.UtcNow;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _selectedSensorType = "Air";
        public string SelectedSensorType
        {
            get => _selectedSensorType;
            set => SetProperty(ref _selectedSensorType, value);
        }

        private string _selectedRegion = "All";
        public string SelectedRegion
        {
            get => _selectedRegion;
            set => SetProperty(ref _selectedRegion, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // ————————————————————————————————————— Data & Commands
        public ObservableCollection<ChartEntry> ChartData { get; }
        public ObservableCollection<AirQualityRecord> RawData { get; }
        public ICommand LoadDataCommand { get; }

        public Chart Chart
        {
            get
            {
                // both ObservableCollection<ChartEntry> and ChartEntry[] are IEnumerable<ChartEntry>
                IEnumerable<ChartEntry> entries = ChartData.Any()
                    ? ChartData
                    : new[]
                    {
                        new ChartEntry(0f) {
                            Label      = "",
                            ValueLabel = ""
                        }
                    };

                return new LineChart
                {
                    Entries       = entries,
                    LabelTextSize = 24
                };
            }
        }


        async Task LoadDataAsync()
        {
            Debug.WriteLine("⚙️ HistoricalDataViewModel: LoadDataAsync starting");
            if (IsBusy)
            {
                Debug.WriteLine("⚠️ already busy, skipping");
                return;
            }

            try
            {
                IsBusy = true;
                ChartData.Clear();
                RawData.Clear();

                var records = await _dataService
                    .GetAirQualityAsync(StartDate, EndDate, SelectedRegion);
                Debug.WriteLine($"⚙️ got {records.Count} records");

                foreach (var rec in records)
                {
                    var entry = new ChartEntry((float)rec.NO2)
                    {
                        Label      = rec.Timestamp.ToString("MM/dd"),
                        ValueLabel = rec.NO2.ToString("F1")
                    };
                    ChartData.Add(entry);
                    RawData.Add(rec);
                }

                Debug.WriteLine($"⚙️ ChartData now has {ChartData.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadDataAsync failed: {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("⚙️ LoadDataAsync complete");
            }
        }
    }
}
