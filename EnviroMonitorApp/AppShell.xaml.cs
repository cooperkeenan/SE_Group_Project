// AppShell.xaml.cs
using System;
using Microsoft.Maui.Controls;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        readonly IEnvironmentalDataService _dataSvc;
        bool _hasSeeded;

        public AppShell(IEnvironmentalDataService dataSvc)
        {
            InitializeComponent();
            _dataSvc = dataSvc;

            Routing.RegisterRoute(nameof(WaterQualityPage),   typeof(WaterQualityPage));
            Routing.RegisterRoute(nameof(AirQualityPage),     typeof(AirQualityPage));
            Routing.RegisterRoute(nameof(WeatherPage),        typeof(WeatherPage));
            Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_hasSeeded) return;
            _hasSeeded = true;

            // seed, e.g. last 30 days of air data for “London”
            await _dataSvc.GetAirQualityAsync(
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "London");
        }
    }
}
