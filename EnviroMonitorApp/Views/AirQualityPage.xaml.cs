using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class AirQualityPage : ContentPage
    {
        private readonly AirQualityViewModel _vm;

        public AirQualityPage(IEnvironmentalDataService dataService)
        {
            InitializeComponent(); // ← required to parse XAML

            _vm = new AirQualityViewModel(dataService);
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await _vm.LoadAsync(); // this is where the data comes from
            }
            catch (Exception ex)
            {
                await DisplayAlert("Load error", ex.Message, "OK");
            }
        }
    }
}
