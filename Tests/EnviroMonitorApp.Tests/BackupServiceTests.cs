using System;
using System.IO;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;
using SQLite;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Unit tests for <see cref="BackupService"/>.
    /// </summary>
    public class BackupServiceTests : IDisposable
    {
        // -----------------------------------------------------------------
        // Temporary paths created per test run
        // -----------------------------------------------------------------
        private readonly string _dbPath;
        private readonly string _source;
        private readonly string _backup;

        /// <summary>The service under test.</summary>
        private readonly BackupService _svc;

        /// <summary>
        /// Sets up fresh temp folders and a unique SQLite DB for each test.
        /// </summary>
        public BackupServiceTests()
        {
            // ── create a temp “source” folder with four dummy spreadsheets ──
            _source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_source);
            foreach (var f in new[]
                     { "Air_quality.xlsx", "Metadata.xlsx",
                       "Water_quality.xlsx", "Weather.xlsx" })
                File.WriteAllText(Path.Combine(_source, f), "dummy");

            // ── destination folder for the backup copies ──
            _backup = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_backup);

            // ── per‑test SQLite database ──
            _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db3");
            _svc    = new BackupService(_dbPath, _source, _backup);
        }

        /// <summary>
        /// A successful backup should copy all four files and write a
        /// matching record to the database.
        /// </summary>
        [Fact]
        public async Task CreateManualBackupAsync_SucceedsAndCopiesAllFourFiles()
        {
            // act
            var rec = await _svc.CreateManualBackupAsync();

            // assert: status + file count
            Assert.Equal(BackupStatus.Success, rec.Status);
            Assert.Equal(4, Directory.GetFiles(_backup).Length);

            // assert: record persisted to SQLite
            var conn = new SQLiteAsyncConnection(_dbPath);
            var list = await conn.Table<Backup>().ToListAsync();

            Assert.Single(list);
            Assert.Equal(rec.Id, list[0].Id);

            await conn.CloseAsync(); // release handle
        }

        /// <summary>
        /// Deleting the source folder should make the backup fail and
        /// capture the exception message.
        /// </summary>
        [Fact]
        public async Task CreateManualBackupAsync_FailsWhenSourceMissing()
        {
            Directory.Delete(_source, true); // simulate missing data folder

            var rec = await _svc.CreateManualBackupAsync();

            Assert.Equal(BackupStatus.Failure, rec.Status);
            Assert.Contains("Could not find", rec.Details, StringComparison.OrdinalIgnoreCase);
        }

        // -----------------------------------------------------------------
        // Tear‑down
        // -----------------------------------------------------------------

        /// <inheritdoc/>
        public void Dispose()
        {
            TryDeleteDirectory(_source);
            TryDeleteDirectory(_backup);
        }

        /// <summary>Best‑effort delete; swallow locking errors.</summary>
        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                /* ignore — directory already removed or locked */
            }
        }
    }
}
