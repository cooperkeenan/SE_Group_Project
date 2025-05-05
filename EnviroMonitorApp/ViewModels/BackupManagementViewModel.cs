using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.ViewModels
{
    public class BackupManagementViewModel : BindableObject
    {
        readonly IBackupService _backupService;

        public ObservableCollection<Backup> History { get; } = new();

        public ICommand TriggerBackupCommand     { get; }
        public ICommand RefreshHistoryCommand    { get; }
        public ICommand ShareLastBackupCommand   { get; }

        public BackupManagementViewModel(IBackupService backupService)
        {
            _backupService = backupService;

            TriggerBackupCommand   = new Command(async () => await DoBackupAsync());
            RefreshHistoryCommand  = new Command(async () => await LoadHistoryAsync());
            ShareLastBackupCommand = new Command(async () => await ShareLastAsync());

            // initial history load
            _ = LoadHistoryAsync();
        }

        async Task LoadHistoryAsync()
        {
            History.Clear();
            var list = await _backupService.GetBackupHistoryAsync();
            foreach (var b in list)
                History.Add(b);
        }

        async Task DoBackupAsync()
        {
            await _backupService.CreateManualBackupAsync();
            await LoadHistoryAsync();
        }

        async Task ShareLastAsync()
        {
            if (History.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Nothing to Share",
                    "You have no backups yet.",
                    "OK");
                return;
            }

            var last = History[0];
            if (string.IsNullOrWhiteSpace(last.Path) || !Directory.Exists(last.Path))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "No Backup Folder",
                    "Backup folder not found on disk.",
                    "OK");
                return;
            }

            var files = Directory.GetFiles(last.Path);
            if (files.Length == 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Empty Backup",
                    "No files were found in the last backup.",
                    "OK");
                return;
            }

            var shareFiles = files.Select(f => new ShareFile(f)).ToList();
            var request = new ShareMultipleFilesRequest
            {
                Title = "Share Backup Files",
                Files = shareFiles
            };
            await Share.RequestAsync(request);
        }
    }
}
