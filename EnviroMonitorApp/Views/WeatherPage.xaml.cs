// Views/WeatherPage.xaml.cs
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class WeatherPage : ContentPage
    {
        private readonly WeatherViewModel _vm;

        // ← Shell will call THIS one
        public WeatherPage() : this(
            // grab the service from the MAUI DI container:
            App.Current!.Handler.MauiContext.Services.GetRequiredService<IEnvironmentalDataService>())
        { }

        // your DI-friendly ctor (kept for unit tests, manual nav, etc.)
        public WeatherPage(IEnvironmentalDataService svc)
        {
            InitializeComponent();
            _vm = new WeatherViewModel(svc);
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_vm.Forecast.Count == 0)
            {
                try { await _vm.LoadAsync(); }
                catch (Exception ex)
                {
                    await DisplayAlert("Weather error", ex.Message, "OK");
                }
            }
        }
    }
}
