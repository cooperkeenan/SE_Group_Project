

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;

public class AirQualityViewModel : BaseViewModel
{
  readonly IEnvironmentalDataService _svc;
  public ObservableCollection<AirQualityRecord> Items { get; }
    = new ObservableCollection<AirQualityRecord>();

  public AirQualityViewModel(IEnvironmentalDataService svc) => _svc = svc;

  public async Task LoadAsync()
  {
    var data = await _svc.GetAirQualityAsync();
    Items.Clear();
    foreach (var r in data) Items.Add(r);
  }
}
