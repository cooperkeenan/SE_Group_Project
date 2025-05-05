using EnviroMonitor.Core.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    public partial class BackupManagementPage : ContentPage
    {
        public BackupManagementPage(BackupManagementViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ((BackupManagementViewModel)BindingContext).InitializeAsync();
        }
    }
}
