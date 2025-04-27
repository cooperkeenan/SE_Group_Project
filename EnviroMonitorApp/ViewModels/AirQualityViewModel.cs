using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public partial class AirQualityViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _dataService;

        public AirQualityViewModel(IEnvironmentalDataService dataService)
        {
            _dataService = dataService;
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            ChartData = new ObservableCollection<ChartEntry>();
        }

        [ObservableProperty]
        DateTime startDate = DateTime.UtcNow.AddDays(-7);

        [ObservableProperty]
        DateTime endDate = DateTime.UtcNow;

        [ObservableProperty]
        string selectedRegion = string.Empty;

        [ObservableProperty]
        bool isBusy;

        public ObservableCollection<ChartEntry> ChartData { get; }
        public ICommand LoadDataCommand { get; }

        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                ChartData.Clear();

                // ← updated call
                var records = await _dataService
                    .GetAirQualityAsync(StartDate, EndDate, SelectedRegion);

                foreach (var rec in records)
                {
                    ChartData.Add(new ChartEntry((float)rec.NO2)
                    {
                        Label      = rec.Timestamp.ToString("MM/dd"),
                        ValueLabel = rec.NO2.ToString("F1")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AirQuality load failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
