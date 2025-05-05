// MauiProgram.cs
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using System.IO;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using SQLite;

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

            // --- Register Shell & Pages ---
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<SensorConfigurationPage>();
            builder.Services.AddTransient<SensorHistoryPage>();
            builder.Services.AddTransient<BackupManagementPage>();

            // --- SQLite registration ---
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "enviro.db3");
            builder.Services.AddSingleton(_ =>
            {
                var conn = new SQLiteAsyncConnection(dbPath);
                // Ensure the table exists
                conn.CreateTableAsync<Models.Backup>().Wait();
                return conn;
            });

            // --- Backup service & ViewModel ---
            builder.Services.AddSingleton<IBackupService, BackupService>();
            builder.Services.AddTransient<BackupManagementViewModel>();

            return builder.Build();
        }
    }
}