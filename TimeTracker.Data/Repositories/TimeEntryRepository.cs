using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Data.Repositories;

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly TimeTrackerDbContext _context;

    public TimeEntryRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimeEntry>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.TimeEntries
            .Where(te => te.TrackedTaskId == taskId)
            .ToListAsync();
    }

    public async Task AddAsync(TimeEntry timeEntry)
    {
        await _context.TimeEntries.AddAsync(timeEntry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TimeEntry timeEntry)
    {
        if (_context.Entry(timeEntry).State == EntityState.Detached)
        {
            _context.TimeEntries.Update(timeEntry);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entry = await _context.TimeEntries.FindAsync(id);
        if (entry != null)
        {
            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TimeEntry?> GetActiveTimeEntryAsync(Guid taskId)
    {
        return await _context.TimeEntries
            .Where(te => te.TrackedTaskId == taskId && te.EndTime == null)
            .OrderByDescending(te => te.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<TimeEntry?> GetCurrentlyActiveTimeEntryAsync()
    {
        return await _context.TimeEntries
            .Where(te => te.EndTime == null)
            .OrderByDescending(te => te.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TimeEntry>> GetAllAsync()
    {
        return await _context.TimeEntries
            .Include(te => te.TrackedTask)
            .OrderByDescending(te => te.StartTime)
            .ToListAsync();
    }
}
