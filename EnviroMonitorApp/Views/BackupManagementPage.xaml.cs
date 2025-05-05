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
    }
}
