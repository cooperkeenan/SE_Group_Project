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
    /// <summary>
    /// Unit tests for <see cref="BackupManagementViewModel"/>.
    /// </summary>
    public class BackupManagementViewModelTests
    {
        // -----------------------------------------------------------------
        // Hand‑rolled fake — avoids Moq/Castle and keeps the test project light.
        // -----------------------------------------------------------------

        /// <summary>
        /// In‑memory stub that mimics <see cref="IBackupService"/>.
        /// </summary>
        private class FakeBackupService : IBackupService
        {
            /// <summary>
            /// List returned by <see cref="GetBackupHistoryAsync"/>.
            /// Tests mutate this to simulate new data coming in.
            /// </summary>
            public List<Backup> HistoryData { get; set; } = new();

            /// <summary>
            /// Backup instance returned by <see cref="CreateManualBackupAsync"/>.
            /// </summary>
            public Backup CreatedBackup { get; set; } = default!;

            public Task<Backup> CreateManualBackupAsync() =>
                Task.FromResult(CreatedBackup);

            public Task<IReadOnlyList<Backup>> GetBackupHistoryAsync() =>
                Task.FromResult((IReadOnlyList<Backup>)HistoryData);
        }

        // -----------------------------------------------------------------
        // Tests
        // -----------------------------------------------------------------

        /// <summary>
        /// After <see cref="BackupManagementViewModel.InitializeAsync"/> runs,
        /// the <see cref="BackupManagementViewModel.History"/> collection
        /// should contain whatever the service returned, newest first.
        /// </summary>
        [Fact]
        public async Task InitializeAsync_PopulatesHistory()
        {
            var now  = DateTime.UtcNow;
            var fake = new FakeBackupService
            {
                HistoryData = new List<Backup>
                {
                    new() { Id = 1, Timestamp =  now,             Status = BackupStatus.Success },
                    new() { Id = 2, Timestamp =  now.AddMinutes(-5), Status = BackupStatus.Failure }
                }
            };

            var vm = new BackupManagementViewModel(fake);

            await vm.InitializeAsync();

            Assert.Equal(2, vm.History.Count);
            Assert.Equal(1, vm.History.First().Id);
        }

        /// <summary>
        /// Triggering a new backup should call the service and refresh
        /// the history list with the newly created record.
        /// </summary>
        [Fact]
        public async Task TriggerBackupAsync_CallsServiceAndRefreshes()
        {
            var now  = DateTime.UtcNow;
            var fake = new FakeBackupService
            {
                // back‑service will return this when CreateManualBackupAsync is called
                CreatedBackup = new Backup { Id = 99, Timestamp = now, Status = BackupStatus.Pending },

                // start with an empty list so we can verify refresh
                HistoryData = new List<Backup>()
            };

            var vm = new BackupManagementViewModel(fake);
            await vm.InitializeAsync(); // load initial (empty) history

            // simulate the service having a new record after backup
            fake.HistoryData = new List<Backup> { fake.CreatedBackup };

            await vm.TriggerBackupAsync();

            Assert.Single(vm.History);
            Assert.Equal(99, vm.History[0].Id);
        }
    }
}
