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
                var dataService = services.GetRequiredService<IEnvironmentalDataService>();

                // Launch your AirQualityPage inside a nav container
                var page = new AirQualityPage(dataService);
                MainPage = new NavigationPage(page)
                {
                    BarBackgroundColor = Colors.DarkSlateBlue,
                    BarTextColor       = Colors.White
                };
            }
            catch (Exception ex)
            {
                // Display error directly on screen
                MainPage = new ContentPage
                {
                    BackgroundColor = Colors.Black,
                    Content = new ScrollView
                    {
                        Content = new Label
                        {
                            Text = $"⚠️ Startup error:\n\n{ex}",
                            TextColor = Colors.OrangeRed,
                            Padding = new Thickness(20),
                            FontSize = 14
                        }
                    }
                };
            }
        }
    }
}
