// MauiProgram.cs
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

// ← Add this
using EnviroMonitorApp.Views;

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

            // Register Shell
            builder.Services.AddSingleton<AppShell>();

            // Register each page (now resolve correctly)
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<SensorConfigurationPage>();
            builder.Services.AddTransient<SensorHistoryPage>();
            builder.Services.AddTransient<BackupManagementPage>();

            return builder.Build();
        }
    }
}
