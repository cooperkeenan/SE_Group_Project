using Microsoft.Maui.Controls;
using EnviroMonitorApp.Services;


namespace EnviroMonitorApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new Views.MainPage(new ExcelReaderService());

    }
}
