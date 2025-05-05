using System;
using SQLite;

namespace EnviroMonitor.Core.Models
{
    public class Backup
    {
        [PrimaryKey, AutoIncrement]
        public int      Id        { get; set; }

        public DateTime Timestamp { get; set; }

        public BackupStatus Status { get; set; }

        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Folder into which files were backed up.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        public override string ToString() =>
            $"{Timestamp:yyyy-MM-dd HH:mm} → {Status}";
    }
}
