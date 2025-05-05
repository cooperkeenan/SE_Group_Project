using EnviroMonitorApp.Views;
using Microsoft.Maui.Controls;

using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    /// <summary>
    /// Shell class that defines the overall navigation structure of the application.
    /// </summary>
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // If you ever need to register routes in code, you can do it here:
            // Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
        }
    }
}
