using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class WaterQualityPage : ContentPage
    {
        readonly WaterQualityViewModel _vm;

        // MAUI DI will automatically inject WaterQualityViewModel
        public WaterQualityPage(WaterQualityViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Only kick off once per appearance
            if (_vm.WaterQuality.Count == 0)
            {
                _vm.LoadDataCommand.Execute(null);
            }
        }
    }
}
