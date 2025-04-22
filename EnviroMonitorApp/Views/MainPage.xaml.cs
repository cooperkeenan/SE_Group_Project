using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;

namespace EnviroMonitorApp.Views
{
	public partial class MainPage : ContentPage
	{
	readonly WeatherViewModel _vm;

	public MainPage(IEnvironmentalDataService dataService)
		{
			InitializeComponent();
			_vm = new WeatherViewModel(dataService);
			BindingContext = _vm;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _vm.LoadAsync();
		}
	}
}