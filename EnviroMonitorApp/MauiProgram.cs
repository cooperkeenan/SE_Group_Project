using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using EnviroMonitorApp;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;

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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register the OpenAQ client
            builder.Services
                .AddRefitClient<IAirQualityApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.openaq.org"));

            // Register the OpenWeatherMap client
            builder.Services
                .AddRefitClient<IWeatherApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5"));

            // Register the USGS Water Services client
            builder.Services
                .AddRefitClient<IWaterQualityApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://waterservices.usgs.gov/nwis/iv"));

            // Your helpers & aggregate service
            builder.Services.AddSingleton<ApiKeyProvider>();
            builder.Services.AddSingleton<IEnvironmentalDataService, EnvironmentalDataApiService>();

            return builder.Build();
        }
    }
}
