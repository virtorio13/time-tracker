using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Core.Services;

public class SummaryService : ISummaryService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ITaskRepository _taskRepository;

    public SummaryService(ITimeEntryRepository timeEntryRepository, ITaskRepository taskRepository)
    {
        _timeEntryRepository = timeEntryRepository;
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskSummaryDto>> GetTaskSummariesAsync(DateTime start, DateTime end)
    {
        var allEntries = await _timeEntryRepository.GetAllAsync();
        
        // Filter by date range
        var relevantEntries = allEntries.Where(e => 
            e.StartTime >= start && 
            (e.EndTime == null || e.EndTime <= end) // Simplified logic: falls within range if start/end matches
            // Ideally: overlap logic or start time logic. For MVP: simple start time >= range start.
            // Let's refine: Entry intersects with [start, end]?
            // Or simple: Entry started within the range. Let's stick to started within range for now.
            && e.StartTime <= end
        ).ToList();

        // We need to group by Root Task.
        // This requires traversing up the parent chain for each task.
        // To do this efficiently, we might need to load tasks differently or ensure navigation properties are populated.
        // The repository GetAllAsync includes SubTasks, parent/child relationships might be navigatable if loaded.
        // However, TimeEntry has TrackedTaskId. We can get the task and traverse up.

        // Let's fetch all tasks to build a lookup or generic traversal mechanism
        var allTasks = await _taskRepository.GetAllAsync();
        var taskLookup = allTasks.ToDictionary(t => t.Id);

        var summaryMap = new Dictionary<Guid, TimeSpan>();
        var rootTaskNames = new Dictionary<Guid, string>();

        foreach (var entry in relevantEntries)
        {
            if (!taskLookup.TryGetValue(entry.TrackedTaskId, out var task)) continue;

            // Find root
            var root = GetRootTask(task, taskLookup);
            
            // Calculate duration
            var duration = (entry.EndTime ?? DateTime.Now) - entry.StartTime;

            if (summaryMap.ContainsKey(root.Id))
                summaryMap[root.Id] += duration;
            else
            {
                summaryMap[root.Id] = duration;
                rootTaskNames[root.Id] = root.Name;
            }
        }

        return summaryMap.Select(kvp => new TaskSummaryDto
        {
            TaskName = rootTaskNames[kvp.Key],
            TotalDuration = kvp.Value
        }).OrderByDescending(x => x.TotalDuration).ToList();
    }

    private TrackedTask GetRootTask(TrackedTask current, Dictionary<Guid, TrackedTask> lookup)
    {
        while (current.ParentTaskId != null && lookup.TryGetValue(current.ParentTaskId.Value, out var parent))
        {
            current = parent;
        }
        return current;
    }
}
