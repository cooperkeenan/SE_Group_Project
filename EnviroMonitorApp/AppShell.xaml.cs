// EnviroMonitorApp/AppShell.xaml.cs
using System;
using Microsoft.Maui.Controls;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.Services;
using System.Diagnostics;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        readonly EnvironmentalDataApiService _apiSvc;
        readonly SqlDataService               _sqlSvc;
        bool _hasSeeded;

        // Inject both services here
        public AppShell(
            EnvironmentalDataApiService apiSvc,
            SqlDataService sqlSvc)
        {
            InitializeComponent();

            _apiSvc = apiSvc;
            _sqlSvc = sqlSvc;

            Routing.RegisterRoute(nameof(WaterQualityPage),   typeof(WaterQualityPage));
            Routing.RegisterRoute(nameof(AirQualityPage),     typeof(AirQualityPage));
            Routing.RegisterRoute(nameof(WeatherPage),        typeof(WeatherPage));
            Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_hasSeeded)
                return;

            _hasSeeded = true;
            Debug.WriteLine("[AppShell] Starting seed…");

            try
            {
                var from = DateTime.UtcNow.AddDays(-7);
                var to   = DateTime.UtcNow;

                // Fetch each dataset
                var air     = await _apiSvc.GetAirQualityAsync(from, to, "London");
                var weather = await _apiSvc.GetWeatherAsync(from, to, "London");
                var water   = await _apiSvc.GetWaterQualityAsync(from, to, "London");

                Debug.WriteLine($"[AppShell] Fetched {air.Count} air, {weather.Count} weather, {water.Count} water records; seeding DB…");

                // Wipe and seed SQLite
                await _sqlSvc.SeedAsync(air, weather, water);

                Debug.WriteLine("[AppShell] Seed complete.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Seeding failed: {ex}");
            }
        }
    }
}
