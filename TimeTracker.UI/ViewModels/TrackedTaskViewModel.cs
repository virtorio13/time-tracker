using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.UI.ViewModels;

public partial class TrackedTaskViewModel : ObservableObject
{
    private readonly TrackedTask _task;
    private readonly ITimeTrackingService _timeTrackingService;
    private readonly ITaskService _taskService;
    private readonly Action _refreshTreeAction;
    private readonly Action<TrackedTaskViewModel>? _requestSelectionAction;

    [ObservableProperty]
    private string _name;

    partial void OnNameChanged(string value)
    {
        _task.Name = value;
        _taskService.UpdateTaskAsync(_task);
    }

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _durationText;

    public ObservableCollection<TrackedTaskViewModel> SubTasks { get; } = new();
    public ObservableCollection<TodoItemViewModel> TodoItems { get; } = new();

    public Guid Id => _task.Id;

    [ObservableProperty]
    private string _notes;

    [ObservableProperty]
    private string _checklistSummary; 

    [ObservableProperty]
    private string _newTodoText;

    [ObservableProperty]
    private bool _isExpanded = true; 

    public TrackedTaskViewModel(
        TrackedTask task, 
        ITimeTrackingService timeTrackingService,
        ITaskService taskService,
        Action refreshTreeAction,
        Action<TrackedTaskViewModel>? requestSelectionAction = null)
    {
        _task = task;
        _timeTrackingService = timeTrackingService;
        _taskService = taskService;
        _refreshTreeAction = refreshTreeAction;
        _requestSelectionAction = requestSelectionAction;

        Name = task.Name;
        Notes = task.Notes ?? "";
        DurationText = "00:00:00";

        foreach (var subTask in task.SubTasks)
        {
            var subVm = new TrackedTaskViewModel(subTask, timeTrackingService, taskService, refreshTreeAction, requestSelectionAction);
            subVm.PropertyChanged += SubTask_PropertyChanged;
            SubTasks.Add(subVm);
        }
        
        SubTasks.CollectionChanged += (s, e) => {
             if (e.NewItems != null)
                foreach(TrackedTaskViewModel item in e.NewItems) 
                {
                    item.PropertyChanged += SubTask_PropertyChanged;
                    // Trigger refresh immediately when child added
                    RefreshChecklistStats(); 
                }
            if (e.OldItems != null)
                foreach(TrackedTaskViewModel item in e.OldItems) 
                {
                    item.PropertyChanged -= SubTask_PropertyChanged;
                    RefreshChecklistStats();
                }
        };

        foreach (var todo in task.TodoItems)
        {
            var vm = new TodoItemViewModel(todo, taskService);
            vm.PropertyChanged += TodoItem_PropertyChanged;
            TodoItems.Add(vm);
        }
        UpdateChecklistSummary();
        TodoItems.CollectionChanged += (s, e) => {
            if (e.NewItems != null)
                foreach(TodoItemViewModel item in e.NewItems) item.PropertyChanged += TodoItem_PropertyChanged;
            if (e.OldItems != null)
                foreach(TodoItemViewModel item in e.OldItems) item.PropertyChanged -= TodoItem_PropertyChanged;
            UpdateChecklistSummary();
        };
    }

    private void TodoItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TodoItemViewModel.IsDone))
        {
            RefreshChecklistStats();
        }
    }

    private void SubTask_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TotalTodoCount) || e.PropertyName == nameof(CompletedTodoCount))
        {
            RefreshChecklistStats();
        }
    }

    [ObservableProperty]
    private int _completedTodoCount;

    [ObservableProperty]
    private int _totalTodoCount;

    [ObservableProperty]
    private double _progressValue;

    private void UpdateChecklistSummary()
    {
        RefreshChecklistStats();
    }

    public void RefreshChecklistStats()
    {
        // Calculate own stats
        int ownTotal = TodoItems.Count;
        int ownCompleted = TodoItems.Count(t => t.IsDone);

        // Calculate children stats
        int childrenTotal = SubTasks.Sum(s => s.TotalTodoCount);
        int childrenCompleted = SubTasks.Sum(s => s.CompletedTodoCount);

        // Update properties
        TotalTodoCount = ownTotal + childrenTotal;
        CompletedTodoCount = ownCompleted + childrenCompleted;

        if (TotalTodoCount == 0)
        {
            ChecklistSummary = "";
            ProgressValue = 0;
        }
        else
        {
            ChecklistSummary = $"[{CompletedTodoCount}/{TotalTodoCount}]";
            ProgressValue = (double)CompletedTodoCount / TotalTodoCount;
        }
    }
    
    partial void OnNotesChanged(string value)
    {
        _taskService.UpdateTaskNotesAsync(_task.Id, value);
    }

    [RelayCommand]
    private async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTodoText)) return;
        var todo = await _taskService.AddTodoAsync(_task.Id, NewTodoText);
        // TodoItems.Add handled by collection changed logic if deemed necessary, but we add manually here
        // actually existing code adds manually
        // We rely on CollectionChanged to hook up events
        TodoItems.Add(new TodoItemViewModel(todo, _taskService));
        NewTodoText = string.Empty;
    }

    [RelayCommand]
    private async Task RemoveTodo(TodoItemViewModel todo)
    {
        await _taskService.DeleteTodoAsync(_task.Id, todo.Id);
        TodoItems.Remove(todo);
    }

    [RelayCommand]
    private async Task Start()
    {
        await _timeTrackingService.StartTaskAsync(_task.Id);
        IsRunning = true;
        _refreshTreeAction?.Invoke(); // Need to refresh to update other running tasks to stopped
    }

    [RelayCommand]
    private async Task Stop()
    {
        await _timeTrackingService.StopTaskAsync(_task.Id);
        IsRunning = false;
        _refreshTreeAction?.Invoke();
    }

    [RelayCommand]
    private async Task AddSubTask()
    {
        var newTask = await _taskService.CreateTaskAsync("New Subtask", _task.Id);
        var newVm = new TrackedTaskViewModel(newTask, _timeTrackingService, _taskService, _refreshTreeAction, _requestSelectionAction);
        // Event subscription handled by CollectionChanged
        SubTasks.Add(newVm);
        IsExpanded = true;
        _requestSelectionAction?.Invoke(newVm);
    }

    [RelayCommand]
    private async Task Delete()
    {
        await _taskService.DeleteTaskAsync(_task.Id);
        _refreshTreeAction?.Invoke(); // Parent needs to remove this from its collection
    }
    
    private DateTime? _startTime;

    public void UpdateState(TimeEntry? activeEntry)
    {
        if (activeEntry != null && activeEntry.TrackedTaskId == _task.Id)
        {
            IsRunning = true;
            _startTime = activeEntry.StartTime;
        }
        else
        {
            IsRunning = false;
            _startTime = null;
        }
        RefreshDuration();
    }

    public TimeSpan TotalDuration { get; private set; }

    public void RefreshDuration()
    {
        TimeSpan total = TimeSpan.Zero;

        // 1. Own Time
        if (_task.TimeEntries != null)
        {
            foreach(var entry in _task.TimeEntries)
            {
                if (entry.EndTime.HasValue)
                {
                    total += (entry.EndTime.Value - entry.StartTime);
                }
                else
                {
                    // Active entry
                    total += (DateTime.Now - entry.StartTime);
                }
            }
        }

        // 2. Subtasks Time
        foreach(var sub in SubTasks)
        {
            total += sub.TotalDuration;
        }

        TotalDuration = total;
        DurationText = total.ToString(@"hh\:mm\:ss");
    }
}
