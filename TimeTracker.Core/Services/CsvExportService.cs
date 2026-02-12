using System.Text;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Core.Services;

public class CsvExportService : IExportService
{
    private readonly ITimeEntryRepository _timeEntryRepository;

    public CsvExportService(ITimeEntryRepository timeEntryRepository)
    {
        _timeEntryRepository = timeEntryRepository;
    }

    public async Task ExportToCsvAsync(string filePath)
    {
        var entries = await _timeEntryRepository.GetAllAsync();
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Task Name,Start Time,End Time,Duration,Notes");

        foreach (var entry in entries)
        {
            var taskName = entry.TrackedTask?.Name ?? "Unknown Task";
            var start = entry.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            var end = entry.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Running";
            
            var duration = "";
            if (entry.EndTime.HasValue)
            {
                var ts = entry.EndTime.Value - entry.StartTime;
                duration = ts.ToString(@"hh\:mm\:ss");
            }
            else
            {
                var ts = DateTime.Now - entry.StartTime;
                duration = ts.ToString(@"hh\:mm\:ss") + " (Running)";
            }

            var notes = entry.Notes?.Replace(",", ";") ?? ""; 
            // Simple CSV escaping: replace comma with semicolon. 
            // For robust CSV, we should use a library or properly quote fields.
            // I'll add quoting logic.
            
            sb.AppendLine($"{Escape(taskName)},{start},{end},{duration},{Escape(notes)}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }

    private string Escape(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
        {
            return $"\"{input.Replace("\"", "\"\"")}\"";
        }
        return input;
    }
}
