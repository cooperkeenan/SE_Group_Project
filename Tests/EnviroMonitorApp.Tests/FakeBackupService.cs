using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Small in‑memory stand‑in for <see cref="IBackupService"/>.
    /// Used by unit tests so we don’t need SQLite or the file‑system.
    /// </summary>
    public class FakeBackupService : IBackupService
    {
        /// <summary>
        /// Internal list that represents the “database”.
        /// Newest records are inserted at index 0.
        /// </summary>
        public List<Backup> HistoryList { get; } = new();

        /// <summary>
        /// Generates a dummy backup record and pushes it to
        /// <see cref="HistoryList"/>.
        /// </summary>
        public Task<Backup> CreateManualBackupAsync()
        {
            var b = new Backup
            {
                Id        = HistoryList.Count + 1,
                Timestamp = DateTime.UtcNow,
                Status    = BackupStatus.Pending,
                Details   = "fake",
                Path      = "fake-path"
            };

            // put newest first
            HistoryList.Insert(0, b);
            return Task.FromResult(b);
        }

        /// <summary>
        /// Returns the current <see cref="HistoryList"/> as read‑only.
        /// </summary>
        public Task<IReadOnlyList<Backup>> GetBackupHistoryAsync() =>
            Task.FromResult((IReadOnlyList<Backup>)HistoryList);
    }
}
