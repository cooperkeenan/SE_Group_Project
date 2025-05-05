using EnviroMonitor.Core.ViewModels;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.Views
{
    /// <summary>
    /// MAUI page that hosts the backup management UI.
    /// </summary>
    public partial class BackupManagementPage : ContentPage
    {
        /// <summary>
        /// Creates the page and wires up the supplied view‑model.
        /// </summary>
        /// <param name="vm">Instance of <see cref="BackupManagementViewModel"/> injected by DI.</param>
        public BackupManagementPage(BackupManagementViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        /// <summary>
        /// Loads backup history the first time the page appears.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ((BackupManagementViewModel)BindingContext).InitializeAsync();
        }
    }
}
