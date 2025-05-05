using System;
using EnviroMonitor.Core.Models;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class BackupModelTests
    {
        [Fact]
        public void ToString_FormatsCorrectly()
        {
            var ts = new DateTime(2025, 5, 5, 13, 45, 0);
            var b  = new Backup { Timestamp = ts, Status = BackupStatus.Success };

            Assert.Equal("2025-05-05 13:45 → Success", b.ToString());
        }
    }
}
