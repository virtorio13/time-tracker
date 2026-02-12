using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Data.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TimeTrackerDbContext _context;

    public TaskRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrackedTask>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.SubTasks)
            .Include(t => t.TimeEntries)
            .Include(t => t.TodoItems)
            .ToListAsync();
    }

    public async Task<TrackedTask?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.SubTasks)
            .Include(t => t.TimeEntries)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(TrackedTask task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TrackedTask task)
    {
        if (_context.Entry(task).State == EntityState.Detached)
        {
            _context.Tasks.Update(task);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await GetByIdAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
