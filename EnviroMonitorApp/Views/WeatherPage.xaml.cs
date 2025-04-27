using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class WeatherPage : ContentPage
    {
        readonly WeatherViewModel _vm;

        // MAUI will resolve this ctor (WeatherViewModel is registered in MauiProgram)
        public WeatherPage(WeatherViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Only load once per appearance
            if (_vm.ChartData.Count == 0)
            {
                _vm.LoadDataCommand.Execute(null);
            }
        }
    }
}
