using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Very simple in-memory implementation of IBackupService.
    /// </summary>
    public class FakeBackupService : IBackupService
    {
        public List<Backup> HistoryList { get; } = new();

        public Task<Backup> CreateManualBackupAsync()
        {
            // create a predictable “new” Backup
            var b = new Backup {
                Id        = HistoryList.Count + 1,
                Timestamp = DateTime.UtcNow,
                Status    = BackupStatus.Pending,
                Details   = "fake",
                Path      = "fake-path"
            };
            HistoryList.Insert(0, b);
            return Task.FromResult(b);
        }

        public Task<IReadOnlyList<Backup>> GetBackupHistoryAsync() =>
            Task.FromResult((IReadOnlyList<Backup>)HistoryList);
    }
}
