using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;
using EnviroMonitor.Core.ViewModels;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class BackupManagementViewModelTests
    {
        //
        // Tiny in‑file fake replacement for Moq
        //
        class FakeBackupService : IBackupService
        {
            public List<Backup> HistoryData { get; set; } = new();
            public Backup CreatedBackup { get; set; }

            public Task<Backup> CreateManualBackupAsync()
                => Task.FromResult(CreatedBackup);

            public Task<IReadOnlyList<Backup>> GetBackupHistoryAsync()
                => Task.FromResult((IReadOnlyList<Backup>)HistoryData);
        }

        [Fact]
        public async Task InitializeAsync_PopulatesHistory()
        {
            var now = DateTime.UtcNow;
            var fake = new FakeBackupService
            {
                HistoryData = new List<Backup>
                {
                    new() { Id=1, Timestamp=now,             Status=BackupStatus.Success },
                    new() { Id=2, Timestamp=now.AddMinutes(-5), Status=BackupStatus.Failure }
                }
            };
            var vm = new BackupManagementViewModel(fake);

            await vm.InitializeAsync();

            Assert.Equal(2, vm.History.Count);
            Assert.Equal(1, vm.History.First().Id);
        }

        [Fact]
        public async Task TriggerBackupAsync_CallsServiceAndRefreshes()
        {
            var now  = DateTime.UtcNow;
            var fake = new FakeBackupService
            {
                CreatedBackup = new Backup { Id = 99, Timestamp = now, Status = BackupStatus.Pending },
                HistoryData   = new List<Backup>()   // start empty
            };

            var vm = new BackupManagementViewModel(fake);
            await vm.InitializeAsync();             // initial load

            // prepare the history that should be returned *after* backup
            fake.HistoryData = new List<Backup> { fake.CreatedBackup };

            await vm.TriggerBackupAsync();

            Assert.Single(vm.History);
            Assert.Equal(99, vm.History[0].Id);
        }
    }
}
