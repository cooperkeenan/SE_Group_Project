using EnviroMonitorApp.ViewModels;
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
            if (BindingContext is BackupManagementViewModel vm)
                await vm.InitializeAsync();
        }
    }
}