// MauiProgram.cs
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using EnviroMonitorApp;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.ChartTransformers;
using SkiaSharp.Views.Maui.Controls.Hosting;     

namespace EnviroMonitorApp
{
    /// <summary>
    /// Main entry point for the MAUI application.
    /// Configures services, dependency injection, and application defaults.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MAUI application instance.
        /// </summary>
        /// <returns>A configured MAUI application instance.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()                    
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Register data & API services
            builder.Services.AddSingleton<IEnvironmentalDataService, SqlDataService>();

            // Register chart transformer
            builder.Services.AddSingleton<IChartTransformer, LogBinningTransformer>();

            // Register ViewModels
            builder.Services.AddSingleton<HistoricalDataViewModel>();

            // Register Pages
            builder.Services.AddSingleton<HistoricalDataPage>();
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}