using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using SQLite;

namespace EnviroMonitorApp.Services
{
    public class BackupService : IBackupService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly Schedule _schedule = new();

        public BackupService(SQLiteAsyncConnection db)
        {
            _db = db;
            _schedule.CalculateNextRun();
        }

        public async Task<Schedule> GetNextScheduleAsync()
        {
            _schedule.CalculateNextRun();
            return _schedule;
        }

        public async Task<IReadOnlyList<Backup>> GetBackupHistoryAsync()
        {
            return await _db
                .Table<Backup>()
                .OrderByDescending(b => b.Timestamp)
                .ToListAsync();
        }

        public async Task<Backup> CreateManualBackupAsync()
        {
            var backup = new Backup
            {
                Status = BackupStatus.Pending,
                Details = "Starting manual backup..."
            };
            await _db.InsertAsync(backup);

            try
            {
                // Simulate backup work
                await Task.Delay(500);

                backup.Status = BackupStatus.Success;
                backup.Details = "Manual backup completed successfully.";
            }
            catch (Exception ex)
            {
                backup.Status = BackupStatus.Failure;
                backup.Details = ex.Message;
            }

            await _db.UpdateAsync(backup);
            return backup;
        }
    }
}