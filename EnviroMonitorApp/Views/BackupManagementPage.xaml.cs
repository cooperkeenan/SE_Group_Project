using System;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class BackupManagementPage : ContentPage
    {
        public BackupManagementPage()
        {
            InitializeComponent();
        }

        void OnMenuClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
        }
    }
}
