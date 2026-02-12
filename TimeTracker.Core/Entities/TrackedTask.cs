using System.Collections.ObjectModel;

namespace TimeTracker.Core.Entities;

public class TrackedTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    
    public Guid? ParentTaskId { get; set; }
    public virtual TrackedTask? ParentTask { get; set; }
    
    public virtual ICollection<TrackedTask> SubTasks { get; set; } = new List<TrackedTask>();
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}
