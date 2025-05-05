using System;
using SQLite;

namespace EnviroMonitorApp.Models
{
    public class Backup
    {
        [PrimaryKey, AutoIncrement]
        public int          Id        { get; set; }

        public DateTime     Timestamp { get; set; }

        public BackupStatus Status    { get; set; }

        public string       Details   { get; set; } = string.Empty;

        /// <summary>
        /// Path of the folder where files were copied.
        /// </summary>
        public string       Path      { get; set; } = string.Empty;

        public override string ToString() =>
            $"{Timestamp:yyyy-MM-dd HH:mm} → {Status}";
    }
}
