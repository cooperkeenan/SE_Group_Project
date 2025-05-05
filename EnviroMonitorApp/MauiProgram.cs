using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using EnviroMonitorApp.Views;
using EnviroMonitor.Core.ViewModels;
using EnviroMonitor.Core.Services;
using SQLitePCL;            // for Batteries.Init()

namespace EnviroMonitorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // 1) Initialize the native SQLite provider
            Batteries.Init();

            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // 2) Register your Shell, Pages & ViewModels
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<SensorConfigurationPage>();
            builder.Services.AddTransient<SensorHistoryPage>();
            builder.Services.AddTransient<BackupManagementPage>();
            builder.Services.AddTransient<BackupManagementViewModel>();

            //
            // 3) Prepare on-disk folders
            //
            // Base app data folder (sandboxed)
            string appData       = FileSystem.AppDataDirectory;

            // 3a) Copy embedded Excel files from Resources/Raw/data → AppDataDirectory/data
            string sourceFolder  = Path.Combine(appData, "data");
            Directory.CreateDirectory(sourceFolder);

            foreach (var fileName in new[]
            {
                "Air_quality.xlsx",
                "Metadata.xlsx",
                "Water_quality.xlsx",
                "Weather.xlsx"
            })
            {
                // OpenAppPackageFileAsync("data/...") points at Resources/Raw/data/...
                using var embedded = FileSystem.OpenAppPackageFileAsync($"data/{fileName}").Result;
                var destPath = Path.Combine(sourceFolder, fileName);

                // Overwrite if it already exists
                using var destStream = File.Create(destPath);
                embedded.CopyTo(destStream);
            }

            // 3b) Define where backups should go
            string backupFolder  = Path.Combine(appData, "backup_data");
            Directory.CreateDirectory(backupFolder);

            // 3c) Define your SQLite DB path
            string dbPath        = Path.Combine(appData, "enviro.db3");

            //
            // 4) Register the Core BackupService
            //
            //    It will internally create its own SQLiteAsyncConnection(dbPath)
            //    and pull from `sourceFolder`, dumping into `backupFolder`.
            builder.Services.AddSingleton<IBackupService>(_ =>
                new BackupService(dbPath, sourceFolder, backupFolder)
            );

            return builder.Build();
        }
    }
}
