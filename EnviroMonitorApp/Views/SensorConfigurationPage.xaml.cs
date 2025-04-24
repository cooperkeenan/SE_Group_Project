using System;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class SensorConfigurationPage : ContentPage
    {
        public SensorConfigurationPage()
        {
            InitializeComponent();
        }

        void OnMenuClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
        }
    }
}
