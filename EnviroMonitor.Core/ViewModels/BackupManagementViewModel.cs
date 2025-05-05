// File: EnviroMonitor.Core/ViewModels/BackupManagementViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EnviroMonitor.Core.Mvvm;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;

namespace EnviroMonitor.Core.ViewModels
{
    public class BackupManagementViewModel : ObservableObject
    {
        readonly IBackupService _backupService;

        public ObservableCollection<Backup> History { get; } = new();

        private ICommand? _triggerBackupCommand;
        public ICommand TriggerBackupCommand =>
            _triggerBackupCommand ??= new RelayCommand(async () => await TriggerBackupAsync());

        private ICommand? _refreshHistoryCommand;
        public ICommand RefreshHistoryCommand =>
            _refreshHistoryCommand ??= new RelayCommand(async () => await RefreshHistoryAsync());

        public BackupManagementViewModel(IBackupService backupService)
        {
            _backupService = backupService;
        }

        /// <summary>
        /// Call on page appearing to do the initial load.
        /// </summary>
        public async Task InitializeAsync()
        {
            await RefreshHistoryAsync();
        }

        /// <summary>
        /// Refreshes the History collection.
        /// </summary>
        public async Task RefreshHistoryAsync()
        {
            History.Clear();
            var list = await _backupService.GetBackupHistoryAsync();
            foreach (var b in list)
                History.Add(b);
        }

        /// <summary>
        /// Triggers a manual backup then reloads history.
        /// </summary>
        public async Task TriggerBackupAsync()
        {
            await _backupService.CreateManualBackupAsync();
            await RefreshHistoryAsync();
        }
    }
}
