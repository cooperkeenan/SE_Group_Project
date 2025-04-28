// EnviroMonitorApp/MauiProgram.cs
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Refit;
using Microcharts.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.ViewModels;

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 1️⃣ Pages
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<AirQualityPage>();
            builder.Services.AddTransient<WeatherPage>();
            builder.Services.AddTransient<WaterQualityPage>();
            builder.Services.AddTransient<HistoricalDataPage>();

            // 2️⃣ ViewModels
            builder.Services.AddTransient<AirQualityViewModel>();
            builder.Services.AddTransient<WeatherViewModel>();
            builder.Services.AddTransient<WaterQualityViewModel>();
            builder.Services.AddTransient<HistoricalDataViewModel>();

            // 3️⃣ API key + logging handler
            builder.Services.AddSingleton<ApiKeyProvider>();
            builder.Services.AddTransient<HttpLoggingHandler>();

            // 4️⃣ Refit clients
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

            // 5️⃣ Raw API service (for seeding)
            builder.Services.AddSingleton<EnvironmentalDataApiService>();

            // 6️⃣ SQLite store & IEnvironmentalDataService
            builder.Services.AddSingleton<SqlDataService>();
            builder.Services.AddSingleton<IEnvironmentalDataService>(sp =>
                sp.GetRequiredService<SqlDataService>());

            // build & route
            var app = builder.Build();
            Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
            return app;
        }
    }
}
