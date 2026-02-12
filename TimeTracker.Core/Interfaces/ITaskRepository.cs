using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITaskRepository
{
    Task<List<TrackedTask>> GetAllAsync();
    Task<TrackedTask?> GetByIdAsync(Guid id);
    Task AddAsync(TrackedTask task);
    Task UpdateAsync(TrackedTask task);
    Task DeleteAsync(Guid id);
}
