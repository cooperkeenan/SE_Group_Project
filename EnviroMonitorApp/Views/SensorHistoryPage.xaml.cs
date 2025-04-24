using System;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class SensorHistoryPage : ContentPage
    {
        public SensorHistoryPage()
        {
            InitializeComponent();
        }

        void OnMenuClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
        }
    }
}
