using Microsoft.Maui.Controls;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Views;

namespace EnviroMonitorApp
{
    public partial class App : Application
    {
        public App(IServiceProvider services)
        {
            InitializeComponent();

            try
            {
                var dataSvc = services.GetRequiredService<IEnvironmentalDataService>();
                MainPage = new NavigationPage(new AirQualityPage(dataSvc))
                {
                    BarBackgroundColor = Colors.DarkSlateBlue,
                    BarTextColor       = Colors.White
                };
            }
            catch (Exception ex)
            {
                // Show the error so we know what’s wrong
                MainPage = new ContentPage
                {
                    Content = new Label
                    {
                        Text = $"Startup exception:\n{ex}",
                        TextColor = Colors.Red,
                        Padding = 20
                    }
                };
            }
        }
    }
}
