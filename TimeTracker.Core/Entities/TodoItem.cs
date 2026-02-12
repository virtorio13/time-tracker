namespace TimeTracker.Core.Entities;

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    
    public Guid TrackedTaskId { get; set; }
    public virtual TrackedTask? TrackedTask { get; set; }
}
