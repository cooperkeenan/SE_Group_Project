using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using SQLite;

namespace EnviroMonitor.Core.Services
{
    public class BackupService : IBackupService
    {
        readonly SQLiteAsyncConnection _db;
        readonly string _sourceFolder;
        readonly string _backupFolder;

        /// <param name="dbPath">Full path to enviro.db3</param>
        /// <param name="sourceFolder">Where the four Excel files live</param>
        /// <param name="backupFolder">Where to copy them</param>
        public BackupService(string dbPath, string sourceFolder, string backupFolder)
        {
            _db           = new SQLiteAsyncConnection(dbPath);
            _sourceFolder = sourceFolder;
            _backupFolder = backupFolder;
        }

        public async Task<Backup> CreateManualBackupAsync()
        {
            await _db.CreateTableAsync<Backup>();

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
                // ensure backup folder exists
                Directory.CreateDirectory(_backupFolder);

                var files = new[]
                {
                    "Air_quality.xlsx",
                    "Metadata.xlsx",
                    "Water_quality.xlsx",
                    "Weather.xlsx"
                };

                foreach (var f in files)
                {
                    var srcPath  = Path.Combine(_sourceFolder, f);
                    var destPath = Path.Combine(_backupFolder, f);
                    File.Copy(srcPath, destPath, overwrite: true);
                }

                record.Status  = BackupStatus.Success;
                record.Details = $"Copied {files.Length} files.";
                record.Path    = _backupFolder;
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
