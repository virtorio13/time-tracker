namespace TimeTracker.Core.Entities;

public class TimeEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TrackedTaskId { get; set; }
    public virtual TrackedTask? TrackedTask { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public string? Notes { get; set; }
}
