using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITimeTrackingService
{
    Task StartTaskAsync(Guid taskId);
    Task StopTaskAsync(Guid taskId); // Used for Pause as well? Or separate Pause?
    // Requirement says "Start button... display a Stop and Pause button".
    // Pause behavior usually keeps the "session" alive but stops the clock.
    // Or simpler: Stop ends the entry. Start starts a new one. 
    // If Pause is distinct, it creates a gap. 
    // Let's implement Pause as Stop for now (creates an endTime), and Resume as Start.
    // So StopTaskAsync effectively stops the timer.
    
    Task<TimeEntry?> GetActiveTimeEntryAsync(Guid taskId);
    Task<bool> IsTaskActiveAsync(Guid taskId);
}
