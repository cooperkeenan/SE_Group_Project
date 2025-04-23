using Microsoft.Maui.Controls;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;

namespace EnviroMonitorApp.Views
{
    public partial class AirQualityPage : ContentPage
    {
        readonly AirQualityViewModel _vm;

        public AirQualityPage(IEnvironmentalDataService dataService)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                // If XAML load fails, show it
                Content = new Label
                {
                    Text = $"XAML init failed:\n{ex}",
                    TextColor = Colors.Red,
                    Padding = 20
                };
                return;
            }

            _vm = new AirQualityViewModel(dataService);
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _vm.LoadAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Load error", ex.Message, "OK");
            }
        }
    }
}
