using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Core.Services;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public TimeTrackingService(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task StartTaskAsync(Guid taskId)
    {
        // Stop any currently active task
        var activeEntry = await _timeEntryRepository.GetCurrentlyActiveTimeEntryAsync();
        if (activeEntry != null)
        {
            if (activeEntry.TrackedTaskId == taskId)
            {
                return; // Already running
            }
            await StopTaskAsync(activeEntry.TrackedTaskId);
        }

        var newEntry = new TimeEntry
        {
            TrackedTaskId = taskId,
            StartTime = DateTime.Now
        };

        await _timeEntryRepository.AddAsync(newEntry);
    }

    public async Task StopTaskAsync(Guid taskId)
    {
        var activeEntry = await _timeEntryRepository.GetActiveTimeEntryAsync(taskId);
        if (activeEntry != null)
        {
            activeEntry.EndTime = DateTime.Now;
            await _timeEntryRepository.UpdateAsync(activeEntry);
        }
    }

    public async Task<TimeEntry?> GetActiveTimeEntryAsync(Guid taskId)
    {
        return await _timeEntryRepository.GetActiveTimeEntryAsync(taskId);
    }

    public async Task<bool> IsTaskActiveAsync(Guid taskId)
    {
        var entry = await GetActiveTimeEntryAsync(taskId);
        return entry != null;
    }
}
