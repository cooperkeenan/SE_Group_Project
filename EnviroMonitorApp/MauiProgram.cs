// MauiProgram.cs
using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;           // ← for FileSystem.AppDataDirectory
using SQLite;                           // ← for SQLiteAsyncConnection
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
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(f =>
                {
                    f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // ─── Platform / FileSystem service ───────────────────────────────────────────
            builder.Services.AddSingleton<IFileSystemService, MauiFileSystemService>();

            // ─── Register SQLiteAsyncConnection so DI knows what it is ────────────────────
            builder.Services.AddSingleton(sp =>
            {
                // pull the AppDataDirectory path from our file‐system abstraction
                var folder = sp.GetRequiredService<IFileSystemService>().AppDataDirectory;
                var dbPath = Path.Combine(folder, "enviro.db3");
                return new SQLiteAsyncConnection(dbPath);
            });

            // ─── Core Data & API services ───────────────────────────────────────────────
            builder.Services.AddSingleton<IEnvironmentalDataService, SqlDataService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IChartTransformer, LogBinningTransformer>();

            // ─── UI: view‐models & pages ─────────────────────────────────────────────────
            builder.Services.AddTransient<HistoricalDataViewModel>();
            builder.Services.AddTransient<HistoricalDataPage>();

            // ─── Shell ─────────────────────────────────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
