using System;
using SQLite;

namespace EnviroMonitor.Core.Models
{
    /// <summary>
    /// Represents a single backup record created by the
    /// application’s <c>IBackupService</c>.  
    /// The class is mapped to SQLite via <see cref="PrimaryKeyAttribute"/>
    /// and <see cref="AutoIncrementAttribute"/>.
    /// </summary>
    public class Backup
    {
        /// <summary>
        /// Auto‑incrementing primary key.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Coordinated Universal Time when the backup was executed.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Outcome of the backup operation —
        /// e.g. <see cref="BackupStatus.Success"/>,
        /// <see cref="BackupStatus.Failure"/>, or
        /// <see cref="BackupStatus.Pending"/>.
        /// </summary>
        public BackupStatus Status { get; set; }

        /// <summary>
        /// Optional human‑readable details such as an error message
        /// returned by the backup routine.
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// File‑system location of the generated backup archive or folder.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Formats the record as <c>"YYYY‑MM‑DD HH:mm → Status"</c>.
        /// </summary>
        public override string ToString() =>
            $"{Timestamp:yyyy-MM-dd HH:mm} → {Status}";
    }
}
