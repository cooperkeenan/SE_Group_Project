using System;
using System.IO;
using System.Threading.Tasks;
using EnviroMonitor.Core.Models;
using EnviroMonitor.Core.Services;
using SQLite;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class BackupServiceTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly string _source;
        private readonly string _backup;
        private readonly BackupService _svc;

        public BackupServiceTests()
        {
            // ── temp input folder with four dummy spreadsheets ──
            _source = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_source);
            foreach (var f in new[]
                     { "Air_quality.xlsx", "Metadata.xlsx",
                       "Water_quality.xlsx", "Weather.xlsx" })
                File.WriteAllText(Path.Combine(_source, f), "dummy");

            // ── temp output folder ──
            _backup = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_backup);

            // ── unique SQLite db per test run ──
            _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db3");
            _svc = new BackupService(_dbPath, _source, _backup);
        }

        [Fact]
        public async Task CreateManualBackupAsync_SucceedsAndCopiesAllFourFiles()
        {
            var rec = await _svc.CreateManualBackupAsync();

            Assert.Equal(BackupStatus.Success, rec.Status);
            Assert.Equal(4, Directory.GetFiles(_backup).Length);

            // open connection, query, then close — no using/await‑using
            var conn = new SQLiteAsyncConnection(_dbPath);
            var list = await conn.Table<Backup>().ToListAsync();

            Assert.Single(list);
            Assert.Equal(rec.Id, list[0].Id);

            await conn.CloseAsync();   // release handle we opened
        }

        [Fact]
        public async Task CreateManualBackupAsync_FailsWhenSourceMissing()
        {
            Directory.Delete(_source, true);

            var rec = await _svc.CreateManualBackupAsync();

            Assert.Equal(BackupStatus.Failure, rec.Status);
            Assert.Contains("Could not find", rec.Details, StringComparison.OrdinalIgnoreCase);
        }

        // ───────── teardown ─────────
        // Remove the two temp directories; leave the .db3 file in %TEMP%
        // because SQLite keeps a global lock on it.
        public void Dispose()
        {
            TryDeleteDirectory(_source);
            TryDeleteDirectory(_backup);
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                /* ignore – folder still in use or already gone */
            }
        }
    }
}
