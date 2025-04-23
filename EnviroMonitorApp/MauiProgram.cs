using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Refit;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // register API key provider
            builder.Services.AddSingleton<ApiKeyProvider>();

            // Refit clients for external APIs
            builder.Services
                .AddRefitClient<IAirQualityApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.openaq.org"));

            builder.Services
                .AddRefitClient<IWeatherApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5"));

            builder.Services
                .AddRefitClient<IWaterQualityApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://enviro.waterservice.gov/iv"));

            // core data service
            builder.Services.AddSingleton<IEnvironmentalDataService, EnvironmentalDataApiService>();

            // view models
            builder.Services.AddTransient<AirQualityViewModel>();

            // pages
            builder.Services.AddTransient<AirQualityPage>(sp =>
            {
                var vm = sp.GetRequiredService<AirQualityViewModel>();
                return new AirQualityPage(sp.GetRequiredService<IEnvironmentalDataService>())
                {
                    BindingContext = vm
                };
            });

            return builder.Build();
        }
    }
}
