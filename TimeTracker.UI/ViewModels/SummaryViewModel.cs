using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.UI.ViewModels;

public partial class SummaryViewModel : ObservableObject
{
    private readonly ISummaryService _summaryService;

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private DateTimeOffset _startDateOffset;
    
    [ObservableProperty]
    private DateTimeOffset _endDateOffset;

    [ObservableProperty]
    private string _totalTimeText = "00:00:00";

    public ObservableCollection<TaskSummaryViewModel> TaskSummaries { get; } = new();

    public ISeries[] Series { get; set; } = Array.Empty<ISeries>();

    public SummaryViewModel(ISummaryService summaryService)
    {
        _summaryService = summaryService;
        
        // Defaults: Last 30 days
        EndDate = DateTime.Today; 
        StartDate = DateTime.Today.AddDays(-30);
        
        // Initialize DateTimeOffsets for DatePickers
        StartDateOffset = new DateTimeOffset(StartDate);
        EndDateOffset = new DateTimeOffset(EndDate);
        
        LoadDataCommand.Execute(null);
    }
    
    partial void OnStartDateOffsetChanged(DateTimeOffset value)
    {
        StartDate = value.Date;
        LoadDataCommand.Execute(null);
    }

    partial void OnEndDateOffsetChanged(DateTimeOffset value)
    {
        EndDate = value.Date;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        // Adjust EndDate to include the full day
        var end = EndDate.AddDays(1).AddTicks(-1);
        
        var summaries = await _summaryService.GetTaskSummariesAsync(StartDate, end);

        TaskSummaries.Clear();
        TimeSpan total = TimeSpan.Zero;
        
        var pieSeries = new ObservableCollection<ISeries>();

        foreach (var s in summaries)
        {
            TaskSummaries.Add(new TaskSummaryViewModel(s));
            total += s.TotalDuration;
            
            pieSeries.Add(new PieSeries<double> 
            { 
                Values = new double[] { s.TotalDuration.TotalHours }, 
                Name = s.TaskName,
                ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {TimeSpan.FromHours(point.Coordinate.PrimaryValue):hh\\:mm}"
            });
        }

        TotalTimeText = $"{((int)total.TotalHours):00}:{total.Minutes:00}:{total.Seconds:00}";
        Series = pieSeries.ToArray();
        OnPropertyChanged(nameof(Series));
    }
}

public class TaskSummaryViewModel
{
    public string Name { get; }
    public string Duration { get; }
    public double TotalHours { get; }

    public TaskSummaryViewModel(TaskSummaryDto dto)
    {
        Name = dto.TaskName;
        Duration = $"{(int)dto.TotalDuration.TotalHours:00}:{dto.TotalDuration.Minutes:00}";
        TotalHours = dto.TotalDuration.TotalHours;
    }
}
