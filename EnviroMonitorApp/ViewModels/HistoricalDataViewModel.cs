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
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public enum SensorType { Air, Weather, Water }

    public class HistoricalDataViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public HistoricalDataViewModel(IEnvironmentalDataService dataService)
        {
            _dataService    = dataService;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            ChartData       = new ObservableCollection<ChartEntry>();
            RawData         = new ObservableCollection<object>();
            SensorTypes     = Enum.GetValues<SensorType>().ToList();
            Regions         = new List<string> { "London" };
            StartDate       = DateTime.UtcNow.AddDays(-7);
            EndDate         = DateTime.UtcNow;
            SelectedSensorType = SensorType.Air;
            SelectedRegion     = "London";
            ChartData.CollectionChanged += (_,__) => OnPropertyChanged(nameof(Chart));
        }

        public List<SensorType> SensorTypes { get; }
        public List<string>     Regions     { get; }

        private DateTime _startDate;
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime _endDate;
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private SensorType _selectedSensorType;
        public SensorType SelectedSensorType { get => _selectedSensorType; set => SetProperty(ref _selectedSensorType, value); }

        private string _selectedRegion;
        public string SelectedRegion { get => _selectedRegion; set => SetProperty(ref _selectedRegion, value); }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ObservableCollection<object>     RawData   { get; }
        public ICommand LoadDataCommand { get; }

        public Chart Chart => new LineChart {
            Entries = ChartData.Any() ? ChartData : new[] { new ChartEntry(0) },
            LabelTextSize = 24
        };

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                ChartData.Clear();
                RawData.Clear();
                switch (SelectedSensorType)
                {
                    case SensorType.Air:
                        await LoadAirAsync();    break;
                    case SensorType.Weather:
                        await LoadWeatherAsync();break;
                    case SensorType.Water:
                        await LoadWaterAsync();  break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadDataAsync failed: {ex}");
            }
            finally { IsBusy = false; }
        }

        private async Task LoadAirAsync()
        {
            var recs = await _dataService.GetAirQualityAsync(StartDate, EndDate, SelectedRegion);
            foreach (var r in recs)
            {
                ChartData.Add(new ChartEntry((float)r.NO2) {
                    Label = r.Timestamp.ToString("MM/dd"),
                    ValueLabel = r.NO2.ToString("F1")
                });
                RawData.Add(r);
            }
        }

        private async Task LoadWeatherAsync()
        {
            var recs = await _dataService.GetWeatherAsync(StartDate, EndDate, SelectedRegion);
            foreach (var r in recs)
            {
                ChartData.Add(new ChartEntry((float)r.Temperature) {
                    Label = r.Timestamp.ToString("MM/dd"),
                    ValueLabel = r.Temperature.ToString("F1")
                });
                RawData.Add(r);
            }
        }

        private async Task LoadWaterAsync()
        {
            var recs = await _dataService.GetWaterQualityAsync(StartDate, EndDate, SelectedRegion);
            foreach (var r in recs)
            {
                ChartData.Add(new ChartEntry((float)(r.DissolvedOxygen ?? 0)) {
                    Label = r.Timestamp.ToString("MM/dd"),
                    ValueLabel = (r.DissolvedOxygen ?? 0).ToString("F1")
                });
                RawData.Add(r);
            }
        }
    }
}
