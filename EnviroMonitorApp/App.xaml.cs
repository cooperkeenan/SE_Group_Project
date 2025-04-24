// App.xaml.cs
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    public partial class App : Application
    {
        public App(AppShell shell)   // ← just inject the shell
        {
            InitializeComponent();
            MainPage = shell;        // the Shell is now the root page
        }
    }
}
