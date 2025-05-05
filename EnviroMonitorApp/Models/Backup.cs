using System;
using SQLite;

namespace EnviroMonitorApp.Models
{
    [Table("Backups")]
    public class Backup
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public BackupStatus Status { get; set; }
        public string Details { get; set; } = string.Empty;

        public override string ToString() =>
            $"{Timestamp:yyyy-MM-dd HH:mm} [{Status}]";
    }
}