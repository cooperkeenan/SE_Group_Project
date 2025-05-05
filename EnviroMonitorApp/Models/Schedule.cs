using System;

namespace EnviroMonitorApp.Models
{
    public class Schedule
    {
        public string CronExpression { get; set; } = "0 2 * * *"; // Daily at 2 AM
        public DateTime NextRun { get; set; }

        public void CalculateNextRun()
        {
            // Simplest: tomorrow at 02:00 local time
            NextRun = DateTime.Today.AddDays(1).AddHours(2);
        }
    }
}