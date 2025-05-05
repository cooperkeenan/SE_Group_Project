using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using System.IO;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;
using SQLite;
using SQLitePCL;    // for Batteries_V2

namespace EnviroMonitorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Initialize the V2 bundle (this loads dynamic_cdecl + e_sqlite3.so)
            Batteries_V2.Init();

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

            // --- SQLite connection registration ---
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "enviro.db3");
            builder.Services.AddSingleton(_ =>
            {
                var conn = new SQLiteAsyncConnection(dbPath);
                conn.CreateTableAsync<Models.Backup>().Wait();
                return conn;
            });

            // --- Backup service & VM ---
            builder.Services.AddSingleton<IBackupService, BackupService>();
            builder.Services.AddTransient<BackupManagementViewModel>();

            return builder.Build();
        }
    }
}
