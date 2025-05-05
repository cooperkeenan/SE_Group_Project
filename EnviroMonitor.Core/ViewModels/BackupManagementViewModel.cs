using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EnviroMonitor.Core.Mvvm;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;

namespace EnviroMonitor.Core.ViewModels
{
    /// <summary>
    /// View‑model that lets the user create backups
    /// and browse the backup history.
    /// </summary>
    public class BackupManagementViewModel : ObservableObject
    {
        // -----------------------------------------------------------------
        // Constructor‑injected service
        // -----------------------------------------------------------------
        private readonly IBackupService _backupService;

        // -----------------------------------------------------------------
        // Public properties
        // -----------------------------------------------------------------

        /// <summary>
        /// Observable list shown in the UI (newest first).
        /// </summary>
        public ObservableCollection<Backup> History { get; } = new();

        // -----------------------------------------------------------------
        // Commands
        // -----------------------------------------------------------------

        private ICommand? _triggerBackupCommand;
        /// <summary>
        /// Runs <see cref="TriggerBackupAsync"/> when the user presses “Backup Now”.
        /// </summary>
        public ICommand TriggerBackupCommand =>
            _triggerBackupCommand ??= new RelayCommand(async () => await TriggerBackupAsync());

        private ICommand? _refreshHistoryCommand;
        /// <summary>
        /// Refreshes the <see cref="History"/> list from the database.
        /// </summary>
        public ICommand RefreshHistoryCommand =>
            _refreshHistoryCommand ??= new RelayCommand(async () => await RefreshHistoryAsync());

        // -----------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates the view‑model.
        /// </summary>
        /// <param name="backupService">Concrete implementation injected by DI.</param>
        public BackupManagementViewModel(IBackupService backupService)
        {
            _backupService = backupService;
        }

        // -----------------------------------------------------------------
        // Public workflow methods
        // -----------------------------------------------------------------

        /// <summary>
        /// Call once from the view (e.g. OnLoaded) to populate
        /// the initial backup history.
        /// </summary>
        public async Task InitializeAsync() => await RefreshHistoryAsync();

        /// <summary>
        /// Clears <see cref="History"/> then loads records
        /// from <see cref="_backupService"/>.
        /// </summary>
        public async Task RefreshHistoryAsync()
        {
            History.Clear();
            var list = await _backupService.GetBackupHistoryAsync();
            foreach (var b in list)
                History.Add(b);
        }

        /// <summary>
        /// Creates a new backup and immediately refreshes the grid.
        /// </summary>
        public async Task TriggerBackupAsync()
        {
            await _backupService.CreateManualBackupAsync();
            await RefreshHistoryAsync();
        }
    }
}
