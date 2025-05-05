using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EnviroMonitorApp.Models;
using EnviroMonitorApp.Services;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp.ViewModels
{
    public class BackupManagementViewModel : BindableObject
    {
        private readonly IBackupService _backupService;

        public ObservableCollection<Backup> History { get; } = new();
        private Schedule _nextSchedule = new();
        public Schedule NextSchedule
        {
            get => _nextSchedule;
            set { _nextSchedule = value; OnPropertyChanged(); }
        }

        public ICommand TriggerBackupCommand { get; }
        public ICommand RefreshHistoryCommand { get; }

        public BackupManagementViewModel(IBackupService backupService)
        {
            _backupService = backupService;
            TriggerBackupCommand = new Command(async () => await DoBackupAsync());
            RefreshHistoryCommand = new Command(async () => await LoadHistoryAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadScheduleAsync();
            await LoadHistoryAsync();
        }

        private async Task LoadScheduleAsync() =>
            NextSchedule = await _backupService.GetNextScheduleAsync();

        private async Task LoadHistoryAsync()
        {
            History.Clear();
            var list = await _backupService.GetBackupHistoryAsync();
            foreach (var b in list)
                History.Add(b);
        }

        private async Task DoBackupAsync()
        {
            var backup = await _backupService.CreateManualBackupAsync();
            History.Insert(0, backup);
            await LoadScheduleAsync();
        }
    }
}