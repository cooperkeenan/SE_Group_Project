using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;

namespace EnviroMonitor.Core.Services
{
    /// <summary>
    /// Defines the operations a backup service must support.
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// Executes a one‑off backup immediately and returns the
        /// resulting <see cref="Backup"/> record.
        /// </summary>
        Task<Backup> CreateManualBackupAsync();

        /// <summary>
        /// Retrieves all recorded backups, newest first.
        /// </summary>
        Task<IReadOnlyList<Backup>> GetBackupHistoryAsync();
    }
}
