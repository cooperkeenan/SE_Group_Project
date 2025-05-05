using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    public interface IBackupService
    {
        /// <summary>
        /// Copies the four Excel files from Resources/Raw/data into
        /// AppDataDirectory/backup_data and records a Backup entry.
        /// </summary>
        Task<Backup> CreateManualBackupAsync();

        /// <summary>
        /// Retrieves all past backups in descending timestamp order.
        /// </summary>
        Task<IReadOnlyList<Backup>> GetBackupHistoryAsync();
    }
}
