using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using EnviroMonitorApp.Views;         
using EnviroMonitorApp.Services;    
using EnviroMonitorApp.ViewModels;


namespace EnviroMonitorApp;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App(IServiceProvider services)
    {
        InitializeComponent();
        Services = services;

        // after: resolve the interface, not the concrete reader
		var dataService = services.GetRequiredService<IEnvironmentalDataService>();
		MainPage = new NavigationPage(new MainPage(dataService));


    }

    public static IServiceProvider ServiceProvider =>
        ((App)Current!).Services!;
}
