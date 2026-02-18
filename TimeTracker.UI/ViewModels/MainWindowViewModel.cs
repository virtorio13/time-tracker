using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Core.Entities;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly ITimeTrackingService _timeTrackingService;
    private readonly IExportService _exportService;
    private readonly ITimeEntryRepository _timeEntryRepository; // Direct access for active check

    public ObservableCollection<TrackedTaskViewModel> RootTasks { get; } = new();

    [ObservableProperty]
    private TrackedTaskViewModel? _selectedTask;

    public MainWindowViewModel(
        ITaskService taskService, 
        ITimeTrackingService timeTrackingService, 
        IExportService exportService,
        ITimeEntryRepository timeEntryRepository)
    {
        _taskService = taskService;
        _timeTrackingService = timeTrackingService;
        _exportService = exportService;
        _timeEntryRepository = timeEntryRepository;
        
        LoadTasksCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadTasks()
    {
        // Save expansion state
        var expandedIds = new HashSet<Guid>();
        void CreateExpandedSet(IEnumerable<TrackedTaskViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.IsExpanded) expandedIds.Add(node.Id);
                CreateExpandedSet(node.SubTasks);
            }
        }
        CreateExpandedSet(RootTasks);

        var tasks = await _taskService.GetAllTasksAsync();
        var activeEntry = await _timeEntryRepository.GetCurrentlyActiveTimeEntryAsync();

        RootTasks.Clear();
        foreach (var task in tasks.Where(t => t.ParentTaskId == null))
        {
            var vm = new TrackedTaskViewModel(task, _timeTrackingService, _taskService, () => LoadTasksCommand.Execute(null), (selected) => SelectedTask = selected);
            UpdateRecursive(vm, activeEntry);

            // Restore state recursively
            void RestoreState(TrackedTaskViewModel node)
            {
                if (expandedIds.Contains(node.Id)) node.IsExpanded = true;
                foreach(var sub in node.SubTasks) RestoreState(sub);
            }
            RestoreState(vm);

            RootTasks.Add(vm);
        }

        // Star timer if not started
        StartTimer();
    }
    
    private void UpdateRecursive(TrackedTaskViewModel vm, TimeEntry? activeEntry)
    {
        vm.UpdateState(activeEntry);
        foreach(var sub in vm.SubTasks)
        {
            UpdateRecursive(sub, activeEntry);
        }
    }

    private System.Timers.Timer _timer;
    private void StartTimer()
    {
        if (_timer == null)
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => 
            {
                foreach(var root in RootTasks)
                {
                    RefreshDurationRecursive(root);
                }
            };
            _timer.Start();
        }
    }

    private void RefreshDurationRecursive(TrackedTaskViewModel vm)
    {
        foreach(var sub in vm.SubTasks)
        {
             RefreshDurationRecursive(sub);
        }
        vm.RefreshDuration();
    }

    [RelayCommand]
    private async Task AddRootTask()
    {
        await _taskService.CreateTaskAsync("New Task");
        await LoadTasks();
    }

    public Func<Task<string?>>? ShowSaveFileDialog { get; set; }

    [RelayCommand]
    private async Task ExportCsv()
    {
        if (ShowSaveFileDialog == null) return;

        var path = await ShowSaveFileDialog();
        if (!string.IsNullOrEmpty(path))
        {
            await _exportService.ExportToCsvAsync(path);
        }
    }
    
    public Action? ShowSummaryWindow { get; set; }

    [RelayCommand]
    private void OpenSummary()
    {
        ShowSummaryWindow?.Invoke();
    }
}
