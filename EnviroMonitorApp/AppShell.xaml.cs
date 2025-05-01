using Microsoft.Maui.Controls;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.Services;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        readonly IEnvironmentalDataService _dataService;

        public AppShell(IEnvironmentalDataService dataService)
        {
            InitializeComponent();

            _dataService = dataService;

            // Register your routes
            Routing.RegisterRoute(nameof(WaterQualityPage),   typeof(WaterQualityPage));
            Routing.RegisterRoute(nameof(AirQualityPage),     typeof(AirQualityPage));
            Routing.RegisterRoute(nameof(WeatherPage),        typeof(WeatherPage));
            Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
        }

        // If you're not seeding or doing anything else at startup,
        // you can remove this override entirely. Leaving it empty is fine:
        protected override void OnAppearing()
        {
            base.OnAppearing();
            // no more SeedAsync calls — we ship the pre-populated DB now
        }
    }
}
