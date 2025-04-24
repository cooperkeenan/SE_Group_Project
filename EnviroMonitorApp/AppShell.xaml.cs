// AppShell.xaml.cs
using Microsoft.Maui.Controls;
using EnviroMonitorApp.Views;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
			Routing.RegisterRoute(nameof(WaterQualityPage), typeof(WaterQualityPage));

            // optional explicit routes (not needed for simple tabs)
            Routing.RegisterRoute(nameof(AirQualityPage), typeof(AirQualityPage));
            Routing.RegisterRoute(nameof(WeatherPage),    typeof(WeatherPage));
        }
    }
}
