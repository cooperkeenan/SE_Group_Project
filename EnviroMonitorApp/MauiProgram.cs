using Microsoft.Extensions.Logging;
using Refit;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Services.Apis;
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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<AppShell>();       // root
		builder.Services.AddTransient<AirQualityPage>(); // tab pages
		builder.Services.AddTransient<WeatherPage>();
		builder.Services.AddTransient<WaterQualityPage>();
		builder.Services.AddTransient<WaterQualityViewModel>();



        // your key provider
        builder.Services.AddSingleton<ApiKeyProvider>();

		builder.Services.AddTransient<HttpLoggingHandler>();


        // OpenAQ v3 client — inject your X-API-Key header here
        builder.Services
			.AddRefitClient<IAirQualityApi>()
			.AddHttpMessageHandler<HttpLoggingHandler>()
			.ConfigureHttpClient(c =>
			{
				c.BaseAddress = new Uri("https://api.openaq.org/");
				var kp = builder.Services
								.BuildServiceProvider()
								.GetRequiredService<ApiKeyProvider>();
				c.DefaultRequestHeaders.Add("X-API-Key", kp.OpenAqKey);
			});

        // OpenWeatherMap
        builder.Services
            .AddRefitClient<IWeatherApi>()
			.AddHttpMessageHandler<HttpLoggingHandler>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://api.openweathermap.org/");
            });

		builder.Services
			.AddRefitClient<IWaterQualityApi>()
			.ConfigureHttpClient(c =>
			{
				c.BaseAddress = new Uri("https://environment.data.gov.uk/");
			});


        // register your data service
        builder.Services
            .AddSingleton<IEnvironmentalDataService, EnvironmentalDataApiService>();
		


        return builder.Build();
    }
}

