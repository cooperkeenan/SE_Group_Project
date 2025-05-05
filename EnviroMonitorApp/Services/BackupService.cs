using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;
using Microsoft.Maui.Storage;
using SQLite;

namespace EnviroMonitorApp.Services
{
    public class BackupService : IBackupService
    {
        readonly SQLiteAsyncConnection _db;

        public BackupService(SQLiteAsyncConnection db)
            => _db = db;

        public async Task<Backup> CreateManualBackupAsync()
        {
            // ensure table exists
            await _db.CreateTableAsync<Backup>();

            // create record
            var record = new Backup
            {
                Timestamp = DateTime.UtcNow,
                Status    = BackupStatus.Pending,
                Details   = "Starting backup...",
                Path      = string.Empty
            };
            await _db.InsertAsync(record);

            try
            {
                // prepare target folder
                var appData   = FileSystem.AppDataDirectory;
                var backupDir = Path.Combine(appData, "backup_data");
                Directory.CreateDirectory(backupDir);

                // the four Excel files in Resources/Raw/data/
                var files = new[]
                {
                    "Air_quality.xlsx",
                    "Metadata.xlsx",
                    "Water_quality.xlsx",
                    "Weather.xlsx"
                };

                // copy each
                foreach (var f in files)
                {
                    using var src  = await FileSystem.OpenAppPackageFileAsync($"data/{f}");
                    var destPath   = Path.Combine(backupDir, f);
                    using var dest = File.OpenWrite(destPath);
                    await src.CopyToAsync(dest);
                }

                record.Status  = BackupStatus.Success;
                record.Details = $"Copied {files.Length} files.";
                record.Path    = backupDir;
            }
            catch (Exception ex)
            {
                record.Status  = BackupStatus.Failure;
                record.Details = ex.Message;
            }

            await _db.UpdateAsync(record);
            return record;
        }

        public Task<IReadOnlyList<Backup>> GetBackupHistoryAsync() =>
            _db.Table<Backup>()
               .OrderByDescending(b => b.Timestamp)
               .ToListAsync()
               .ContinueWith(t => (IReadOnlyList<Backup>)t.Result);
    }
}
