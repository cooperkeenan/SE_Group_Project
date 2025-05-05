namespace EnviroMonitor.Core.Models
{
    /// <summary>
    /// Indicates the outcome (or current state) of a backup operation.
    /// </summary>
    public enum BackupStatus
    {
        /// <summary>
        /// The backup completed without errors and is usable.
        /// </summary>
        Success,

        /// <summary>
        /// The backup failed; see the accompanying
        /// <c>Details</c> field on <see cref="Backup"/> for the reason.
        /// </summary>
        Failure,

        /// <summary>
        /// The backup has been requested but has not yet finished
        /// (applies to asynchronous operations).
        /// </summary>
        Pending
    }
}
