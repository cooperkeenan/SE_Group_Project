using System;
using Microsoft.Maui.Controls;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        readonly SqlDataService            _sql;    // concrete
        readonly IEnvironmentalDataService _api;    // the interface

        bool _hasSeeded;

        public AppShell(
            IEnvironmentalDataService apiSvc,  // will actually be your SqlDataService under the hood
            SqlDataService sqlSvc)             // also ask for the concrete so you can call SeedAsync
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

            if (!_hasSeeded)
            {
                _hasSeeded = true;

                var from = DateTime.UtcNow.AddDays(-30);
                var to   = DateTime.UtcNow;
                // call your SqlDataService.SeedAsync, not "sqlSvc" which local name didn’t exist
                await _sql.SeedAsync(from, to, "London");
            }
        }
    }
}
