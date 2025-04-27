// ViewModels/WaterQualityViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
    public class WaterQualityViewModel : INotifyPropertyChanged
    {
        private readonly IEnvironmentalDataService? _svc;

        // collection the XAML binds to  ──────────────┐
        public ObservableCollection<WaterQualityRecord> WaterQuality { get; } = new();
                                                     // └───────────────────────────────

        // design-time ctor (no DI)
        public WaterQualityViewModel() { }

        // runtime ctor (DI)
        public WaterQualityViewModel(IEnvironmentalDataService svc) => _svc = svc;

        public async Task LoadAsync()
        {
            if (_svc is null) return;          // design-time

            var list = await _svc.GetWaterQualityAsync(24);
            System.Diagnostics.Debug.WriteLine($"[WQ] fetched {list.Count} record(s)");

            WaterQuality.Clear();
            foreach (var rec in list)
                WaterQuality.Add(rec);
        }

        // INotifyPropertyChanged (not strictly needed for ObservableCollection,
        // but handy if you later expose other simple props)
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
