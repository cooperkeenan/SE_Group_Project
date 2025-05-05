using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;

namespace EnviroMonitor.Core.Services
{
    public interface IBackupService
    {
        /// <summary>
        /// Copies the four Excel files from the app package into 
        /// the backup folder, then records a Backup row.
        /// </summary>
        Task<Backup> CreateManualBackupAsync();

        /// <summary>
        /// All past backups, newest first.
        /// </summary>
        Task<IReadOnlyList<Backup>> GetBackupHistoryAsync();
    }
}
