using EnviroMonitorApp.Views;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    /// <summary>
    /// Shell class that defines the overall navigation structure of the application.
    /// Provides routing between different pages and controls navigation bar appearance.
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Initializes a new instance of the AppShell class.
        /// Sets up shell navigation and routing patterns.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            // We only have one page now, so no additional routing needed.
            // If you still need to navigate by route elsewhere, register it here:
            // Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
        }
    }
}