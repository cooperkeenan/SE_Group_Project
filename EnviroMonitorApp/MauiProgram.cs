using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;    
using EnviroMonitorApp.ViewModels;


namespace EnviroMonitorApp;

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

        // Register pages/services
        builder.Services.AddSingleton<Views.MainPage>();
		builder.Services.AddSingleton<ExcelReaderService>();
    	builder.Services.AddSingleton<IEnvironmentalDataService, EnvironmentalDataService>();


        return builder.Build();
    }
}
