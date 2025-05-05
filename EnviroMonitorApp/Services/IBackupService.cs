using System.Collections.Generic;
using System.Threading.Tasks;
using EnviroMonitorApp.Models;

namespace EnviroMonitorApp.Services
{
    public interface IBackupService
    {
        Task<Backup> CreateManualBackupAsync();
        Task<IReadOnlyList<Backup>> GetBackupHistoryAsync();
        Task<Schedule> GetNextScheduleAsync();
    }
}