using System.IO;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using SQLite;
using SQLitePCL; // for Batteries.Init()

namespace EnviroMonitorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // initialize native SQLite provider
            Batteries.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // register Shell & pages
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<SensorConfigurationPage>();
            builder.Services.AddTransient<SensorHistoryPage>();
            builder.Services.AddTransient<BackupManagementPage>();
            builder.Services.AddTransient<BackupManagementViewModel>();

            // SQLite connection
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "enviro.db3");
            builder.Services.AddSingleton(_ =>
            {
                var conn = new SQLiteAsyncConnection(dbPath);
                conn.CreateTableAsync<Models.Backup>().Wait();
                return conn;
            });

            // backup service
            builder.Services.AddSingleton<IBackupService, BackupService>();

            return builder.Build();
        }
    }

    
}
