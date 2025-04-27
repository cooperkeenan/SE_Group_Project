using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class AirQualityPage : ContentPage
    {
        readonly AirQualityViewModel _vm;

        public AirQualityPage(AirQualityViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_vm.ChartData.Count == 0)
            {
                _vm.LoadDataCommand.Execute(null);
            }
        }
    }
}
