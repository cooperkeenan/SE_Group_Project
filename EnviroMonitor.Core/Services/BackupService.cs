using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using SQLite;

namespace EnviroMonitor.Core.Services
{
    /// <summary>
    /// Handles creating and tracking backups of the four Excel data files.
    /// </summary>
    public class BackupService : IBackupService
    {
        // -----------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------
        private readonly SQLiteAsyncConnection _db;
        private readonly string _sourceFolder;
        private readonly string _backupFolder;

        // -----------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates a new <see cref="BackupService"/>.
        /// </summary>
        /// <param name="dbPath">
        /// Full path to the SQLite database file (<c>enviro.db3</c>).
        /// </param>
        /// <param name="sourceFolder">
        /// Folder that currently contains the four live Excel files.
        /// </param>
        /// <param name="backupFolder">
        /// Destination folder where the copies will be placed.
        /// </param>
        public BackupService(string dbPath, string sourceFolder, string backupFolder)
        {
            _db           = new SQLiteAsyncConnection(dbPath);
            _sourceFolder = sourceFolder;
            _backupFolder = backupFolder;
        }

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>
        /// Copies the four Excel files to <see cref="_backupFolder"/>
        /// and records the outcome in the database.
        /// </summary>
        /// <returns>
        /// A <see cref="Backup"/> record reflecting success or failure.
        /// </returns>
        public async Task<Backup> CreateManualBackupAsync()
        {
            // make sure table exists
            await _db.CreateTableAsync<Backup>();

            // insert initial "pending" record
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

                // success
                record.Status  = BackupStatus.Success;
                record.Details = $"Copied {files.Length} files.";
                record.Path    = _backupFolder;
            }
            catch (Exception ex)
            {
                // failure
                record.Status  = BackupStatus.Failure;
                record.Details = ex.Message;
            }

            await _db.UpdateAsync(record);
            return record;
        }

        /// <summary>
        /// Returns the backup history, newest first.
        /// </summary>
        public Task<IReadOnlyList<Backup>> GetBackupHistoryAsync() =>
            _db.Table<Backup>()
               .OrderByDescending(b => b.Timestamp)
               .ToListAsync()
               .ContinueWith(t => (IReadOnlyList<Backup>)t.Result);
    }
}
