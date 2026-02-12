using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeTracker.Core.Interfaces;

public class TaskSummaryDto
{
    public string TaskName { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
}

public interface ISummaryService
{
    Task<List<TaskSummaryDto>> GetTaskSummariesAsync(DateTime start, DateTime end);
}
