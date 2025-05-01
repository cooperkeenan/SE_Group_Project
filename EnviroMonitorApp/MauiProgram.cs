// MauiProgram.cs
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microcharts.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace EnviroMonitorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMicrocharts()
                .ConfigureFonts(f =>
                {
                    f.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                    f.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ─── Pages & ViewModels ─────────────────────────
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<AirQualityPage>();
            builder.Services.AddTransient<WeatherPage>();
            builder.Services.AddTransient<WaterQualityPage>();
            builder.Services.AddTransient<HistoricalDataPage>();

            builder.Services.AddTransient<AirQualityViewModel>();
            builder.Services.AddTransient<WeatherViewModel>();
            builder.Services.AddTransient<WaterQualityViewModel>();
            builder.Services.AddTransient<HistoricalDataViewModel>();

            // ─── Core data services ──────────────────────────
            // Only register the SQLite-backed provider—no API at runtime:
            builder.Services.AddSingleton<IEnvironmentalDataService, SqlDataService>();

            return builder.Build();
        }
    }
}
