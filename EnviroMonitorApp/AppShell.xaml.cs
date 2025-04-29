// EnviroMonitorApp/AppShell.xaml.cs
using System;
using Microsoft.Maui.Controls;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        readonly SqlDataService           _sql;    // need the concrete type
        readonly IEnvironmentalDataService _api;   // and the interface

        bool _hasSeeded;

        public AppShell(
            IEnvironmentalDataService apiSvc,
            SqlDataService sqlSvc)
        {
            InitializeComponent();

            _api = apiSvc;
            _sql = sqlSvc;

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

            var from = DateTime.UtcNow.AddDays(-30);
            var to   = DateTime.UtcNow;
            // now this will compile, because SeedAsync exists
            await _sql.SeedAsync(from, to, "London");
        }
    }
}
