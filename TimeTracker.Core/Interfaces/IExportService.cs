namespace TimeTracker.Core.Interfaces;

public interface IExportService
{
    Task ExportToCsvAsync(string filePath);
}
