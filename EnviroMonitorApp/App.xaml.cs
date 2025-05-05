using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    /// <summary>
    ///     MAUI <see cref="Application"/> entry point.  
    ///     Sets up resources defined in <c>App.xaml</c> and shows <see cref="AppShell"/>.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        ///     Builds the application and assigns the first page.
        /// </summary>
        public App()
        {
            InitializeComponent();

            // AppShell contains the fly‑out menu and top‑level navigation.
            MainPage = new AppShell();
        }
    }
}
