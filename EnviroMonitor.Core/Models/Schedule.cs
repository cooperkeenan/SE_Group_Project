using System;

namespace EnviroMonitorApp.Models
{
    /// <summary>
    /// Holds a CRON expression and the next time it should run.
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// CRON expression (minute hour day‑of‑month month day‑of‑week).
        /// Default: run every day at 02:00.
        /// </summary>
        public string CronExpression { get; set; } = "0 2 * * *";

        /// <summary>
        /// Next scheduled run in local time.
        /// </summary>
        public DateTime NextRun { get; set; }

        /// <summary>
        /// Sets <see cref="NextRun"/> to tomorrow at 02:00.
        /// </summary>
        public void CalculateNextRun()
        {
            NextRun = DateTime.Today.AddDays(1).AddHours(2);
        }
    }
}
