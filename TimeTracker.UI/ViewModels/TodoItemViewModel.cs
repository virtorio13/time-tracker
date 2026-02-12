using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.UI.ViewModels;

public partial class TodoItemViewModel : ObservableObject
{
    private readonly TodoItem _todoItem;
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private string _description;

    partial void OnDescriptionChanged(string value)
    {
        _todoItem.Description = value;
        _taskService.UpdateTodoAsync(_todoItem);
    }

    [ObservableProperty]
    private bool _isDone;

    public Guid Id => _todoItem.Id;

    public TodoItemViewModel(TodoItem todoItem, ITaskService taskService)
    {
        _todoItem = todoItem;
        _taskService = taskService;
        _description = todoItem.Description;
        _isDone = todoItem.IsDone;
    }

    partial void OnIsDoneChanged(bool value)
    {
        _taskService.ToggleTodoAsync(_todoItem.TrackedTaskId, _todoItem.Id);
    }
}
