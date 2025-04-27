using Microsoft.Extensions.Logging;
using Refit;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
using EnviroMonitorApp.Views;  
using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;   // for Routing

namespace EnviroMonitorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
              .UseMauiApp<App>()
              .ConfigureFonts(fonts =>
              {
                  fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                  fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
              });

    #if DEBUG
            builder.Logging.AddDebug();
    #endif

            //
            // 1️⃣  Pages
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<AirQualityPage>();
            builder.Services.AddTransient<WeatherPage>();
            builder.Services.AddTransient<WaterQualityPage>();
            // builder.Services.AddTransient<HistoricalDataPage>();   // ← comment out until you add that view

            //
            // 2️⃣  ViewModels
            builder.Services.AddTransient<AirQualityViewModel>();
            builder.Services.AddTransient<WeatherViewModel>();
            builder.Services.AddTransient<WaterQualityViewModel>();
            builder.Services.AddTransient<HistoricalDataViewModel>();

            //
            // 3️⃣  API key provider & logging
            builder.Services.AddSingleton<ApiKeyProvider>();
            builder.Services.AddTransient<HttpLoggingHandler>();

            //
            // 4️⃣  Refit clients
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
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("https://environment.data.gov.uk/");
                });

            //
            // 5️⃣  Your unified data‐service
            builder.Services
                .AddSingleton<IEnvironmentalDataService, EnvironmentalDataApiService>();

            var app = builder.Build();

            //Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
            //↑ comment this out too until you actually add that page class

            return app;
        }
    }
}
