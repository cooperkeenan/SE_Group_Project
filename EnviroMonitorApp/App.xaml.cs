using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // We don’t need DI here—just construct the shell.
            MainPage = new AppShell();
        }
    }
}
