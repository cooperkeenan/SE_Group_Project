// MauiProgram.cs
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using EnviroMonitorApp;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.ChartTransformers;
using SkiaSharp.Views.Maui.Controls.Hosting;

// ← Add these two:
using Microsoft.Maui.Storage;   // for FileSystem.AppDataDirectory
using SQLite;                   // for SQLiteAsyncConnection

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
        .ConfigureFonts(f => f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

      // ─── Core services ────────────────────────────────────────────────────

      builder.Services.AddSingleton<IFileSystemService, MauiFileSystemService>();

      // Now the compiler knows what SQLiteAsyncConnection is:
      builder.Services.AddSingleton(sp =>
      {
        var folder = sp.GetRequiredService<IFileSystemService>().AppDataDirectory;
        var path   = Path.Combine(folder, "enviro.db3");
        return new SQLiteAsyncConnection(path);
      });

      builder.Services.AddSingleton<IEnvironmentalDataService, SqlDataService>();
      builder.Services.AddHttpClient();
      builder.Services.AddSingleton<IChartTransformer, LogBinningTransformer>();

      // ─── UI ────────────────────────────────────────────────────────────────

      builder.Services.AddTransient<HistoricalDataViewModel>();
      builder.Services.AddTransient<HistoricalDataPage>();
      builder.Services.AddSingleton<AppShell>();

      return builder.Build();
    }
  }
}
