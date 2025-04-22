using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using EnviroMonitorApp.Views;         
using EnviroMonitorApp.Services;    

namespace EnviroMonitorApp;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App(IServiceProvider services)
    {
        InitializeComponent();
        Services = services;

        MainPage = new NavigationPage(new MainPage(services.GetRequiredService<ExcelReaderService>()));

    }

    public static IServiceProvider ServiceProvider =>
        ((App)Current!).Services!;
}
