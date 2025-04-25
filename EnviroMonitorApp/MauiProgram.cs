using Microsoft.Extensions.Configuration;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;
using EnviroMonitorApp.ViewModels;
using EnviroMonitorApp.Data;
using Microsoft.EntityFrameworkCore;


namespace EnviroMonitorApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        var builder = MauiApp.CreateBuilder();
        
        #if DEBUG
            var appsettingsFile = "appsettings.development.json";
        #else
            var appsettingsFile = "appsettings.json";
        #endif
        
        // Load app settings
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(appsettingsFile)
            .Build();
        
        builder.Configuration.AddConfiguration(config);
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register pages/services
        builder.Services.AddSingleton<Views.MainPage>();
		builder.Services.AddSingleton<ExcelReaderService>();
    	builder.Services.AddSingleton<IEnvironmentalDataService, EnvironmentalDataService>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));


        return builder.Build();
    }
}
