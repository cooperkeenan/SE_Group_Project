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
    public static class MauiProgram
    {
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

            // ─── Data & API services ───────────────────────────────────────
            builder.Services.AddSingleton<IEnvironmentalDataService, SqlDataService>();

            // ─── Chart transformer ─────────────────────────────────────────
            builder.Services.AddSingleton<IChartTransformer, LogBinningTransformer>();

            // ─── ViewModels ────────────────────────────────────────────────
            builder.Services.AddSingleton<HistoricalDataViewModel>();

            // ─── Pages ─────────────────────────────────────────────────────
            builder.Services.AddSingleton<HistoricalDataPage>();
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
