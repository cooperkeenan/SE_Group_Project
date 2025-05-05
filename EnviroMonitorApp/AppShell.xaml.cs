using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    /// <summary>
    /// Top‑level navigation container (tabs, fly‑out, etc.) for the application.
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Builds the shell and loads routes declared in <c>AppShell.xaml</c>.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();
        }
    }
}
