using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using EnviroMonitorApp.Views;
using EnviroMonitor.Core.ViewModels;
using EnviroMonitor.Core.Services;
using SQLitePCL;

namespace EnviroMonitorApp
{
    /// <summary>
    /// Boot‑strapper that builds and configures the MAUI application.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates the <see cref="MauiApp"/> instance with all services,
        /// pages, and view‑models registered in the DI container.
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            //
            // ─── Native SQLite initialisation ───
            //
            Batteries.Init(); // required by SQLitePCLRaw on Android/iOS

            //
            // ─── MAUI builder ───
            //
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()               // root Application
                .ConfigureFonts(fonts =>          // add one custom font
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            //
            // ─── Dependency‑injection registrations ───
            //
            // Shell
            builder.Services.AddSingleton<AppShell>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<SensorConfigurationPage>();
            builder.Services.AddTransient<SensorHistoryPage>();
            builder.Services.AddTransient<BackupManagementPage>();

            // View‑models
            builder.Services.AddTransient<BackupManagementViewModel>();

            //
            // ─── Application data paths ───
            //
            string appData = FileSystem.AppDataDirectory; // platform‑safe folder

            // Location that holds the four live Excel files
            string sourceFolder = Path.Combine(appData, "data");
            Directory.CreateDirectory(sourceFolder);

            // First‑run: copy embedded seed files to the data folder
            foreach (var fileName in new[]
            {
                "Air_quality.xlsx",
                "Metadata.xlsx",
                "Water_quality.xlsx",
                "Weather.xlsx"
            })
            {
                using var embedded  = FileSystem.OpenAppPackageFileAsync($"data/{fileName}").Result;
                var destPath        = Path.Combine(sourceFolder, fileName);

                using var destStream = File.Create(destPath);
                embedded.CopyTo(destStream);
            }

            // Folder where backups will be written
            string backupFolder = Path.Combine(appData, "backup_data");
            Directory.CreateDirectory(backupFolder);

            // SQLite database path
            string dbPath = Path.Combine(appData, "enviro.db3");

            //
            // Register the backup‑service singleton
            //
            builder.Services.AddSingleton<IBackupService>(_ =>
                new BackupService(dbPath, sourceFolder, backupFolder));

            //
            // ─── Build & return the app ───
            //
            return builder.Build();
        }
    }
}
