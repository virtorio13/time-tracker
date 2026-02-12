using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITimeEntryRepository
{
    Task<List<TimeEntry>> GetByTaskIdAsync(Guid taskId);
    Task AddAsync(TimeEntry timeEntry);
    Task UpdateAsync(TimeEntry timeEntry);
    Task DeleteAsync(Guid id);
    Task<TimeEntry?> GetActiveTimeEntryAsync(Guid taskId);
    Task<TimeEntry?> GetCurrentlyActiveTimeEntryAsync();
    Task<List<TimeEntry>> GetAllAsync();
}
