using EnviroMonitorApp.Views;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // We only have one page now, so no additional routing needed.
            // If you still need to navigate by route elsewhere, register it here:
            // Routing.RegisterRoute(nameof(HistoricalDataPage), typeof(HistoricalDataPage));
        }
    }
}
