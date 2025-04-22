// ViewModels/WeatherViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp.ViewModels
{
public class WeatherViewModel : BaseViewModel
{
  readonly IEnvironmentalDataService _dataService;

  public ObservableCollection<WeatherRecord> Items { get; } 
    = new ObservableCollection<WeatherRecord>();

  public WeatherViewModel(IEnvironmentalDataService dataService)
    => _dataService = dataService;

  public async Task LoadAsync()
  {
    var list = await _dataService.GetWeatherAsync();
    Items.Clear();
    foreach(var item in list) Items.Add(item);
  }
}
}