using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public partial class WaterQualityViewModel : ObservableObject
    {
        readonly IEnvironmentalDataService _svc;

        public WaterQualityViewModel(IEnvironmentalDataService svc)
        {
            _svc             = svc;
            WaterQuality     = new ObservableCollection<WaterQualityRecord>();
            LoadDataCommand  = new AsyncRelayCommand(LoadDataAsync);
        }

        [ObservableProperty]
        DateTime startDate = DateTime.UtcNow.AddDays(-7);

        [ObservableProperty]
        DateTime endDate   = DateTime.UtcNow;

        [ObservableProperty]
        string selectedRegion = string.Empty;

        [ObservableProperty]
        bool isBusy;

        /// <summary>
        /// The list your XAML is binding to.
        /// </summary>
        public ObservableCollection<WaterQualityRecord> WaterQuality { get; }

        public IAsyncRelayCommand LoadDataCommand { get; }

        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                WaterQuality.Clear();

                var records = await _svc
                    .GetWaterQualityAsync(StartDate, EndDate, SelectedRegion);

                foreach (var r in records)
                    WaterQuality.Add(r);
            }
            catch (Exception ex)
            {
                // TODO: Show user error, e.g. with a Toast or Alert
                System.Diagnostics.Debug.WriteLine($"LoadWater failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
