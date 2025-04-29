using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Refit;
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

            // ─── API key + logging handler ───────────────────
            builder.Services.AddSingleton<ApiKeyProvider>();
            builder.Services.AddTransient<HttpLoggingHandler>();

            // ─── Refit API clients ───────────────────────────
            builder.Services
                   .AddRefitClient<IAirQualityApi>()
                   .AddHttpMessageHandler<HttpLoggingHandler>()
                   .ConfigureHttpClient((sp, c) =>
                   {
                       var kp = sp.GetRequiredService<ApiKeyProvider>();
                       c.BaseAddress = new Uri("https://api.openaq.org/");
                       c.DefaultRequestHeaders.Add("X-API-Key", kp.OpenAqKey);
                   });

            builder.Services
                   .AddRefitClient<IWeatherApi>()
                   .AddHttpMessageHandler<HttpLoggingHandler>()
                   .ConfigureHttpClient(c =>
                   {
                       c.BaseAddress = new Uri("https://api.openweathermap.org/");
                   });

            builder.Services
                   .AddRefitClient<IWaterQualityApi>()
                   .AddHttpMessageHandler<HttpLoggingHandler>()
                   .ConfigureHttpClient(c =>
                   {
                       c.BaseAddress = new Uri("https://environment.data.gov.uk/");
                   });

            // ─── Core data services ──────────────────────────
            builder.Services.AddSingleton<EnvironmentalDataApiService>(); // the “API‐only” service
            builder.Services.AddSingleton<SqlDataService>();               // concrete SQL
            builder.Services.AddSingleton<IEnvironmentalDataService, SqlDataService>();

            return builder.Build();
        }
    }
}
