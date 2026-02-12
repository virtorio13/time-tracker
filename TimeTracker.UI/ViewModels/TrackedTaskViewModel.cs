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
        Action refreshTreeAction)
    {
        _task = task;
        _timeTrackingService = timeTrackingService;
        _taskService = taskService;
        _refreshTreeAction = refreshTreeAction;

        Name = task.Name;
        Notes = task.Notes ?? "";
        DurationText = "00:00:00";

        foreach (var subTask in task.SubTasks)
        {
            SubTasks.Add(new TrackedTaskViewModel(subTask, timeTrackingService, taskService, refreshTreeAction));
        }

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
            UpdateChecklistSummary();
        }
    }

    private void UpdateChecklistSummary()
    {
        if (TodoItems.Count == 0)
        {
            ChecklistSummary = "";
        }
        else
        {
            int completed = TodoItems.Count(t => t.IsDone);
            ChecklistSummary = $"[{completed}/{TodoItems.Count}]";
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
        SubTasks.Add(new TrackedTaskViewModel(newTask, _timeTrackingService, _taskService, _refreshTreeAction));
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
