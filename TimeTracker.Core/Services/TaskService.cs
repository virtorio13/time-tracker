using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Core.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITodoItemRepository _todoItemRepository;

    public TaskService(ITaskRepository taskRepository, ITodoItemRepository todoItemRepository)
    {
        _taskRepository = taskRepository;
        _todoItemRepository = todoItemRepository;
    }

    public async Task<List<TrackedTask>> GetAllTasksAsync()
    {
        return await _taskRepository.GetAllAsync();
    }

    public async Task<TrackedTask> CreateTaskAsync(string name, Guid? parentId = null)
    {
        var task = new TrackedTask
        {
            Name = name,
            ParentTaskId = parentId
        };
        await _taskRepository.AddAsync(task);
        return task;
    }

    public async Task UpdateTaskAsync(TrackedTask task)
    {
        await _taskRepository.UpdateAsync(task);
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        await _taskRepository.DeleteAsync(id);
    }

    public async Task<TodoItem> AddTodoAsync(Guid taskId, string description)
    {
        var todo = new TodoItem
        {
            TrackedTaskId = taskId,
            Description = description,
            IsDone = false
        };
        
        await _todoItemRepository.AddAsync(todo);
        return todo;
    }

    public async Task ToggleTodoAsync(Guid taskId, Guid todoId)
    {
        var todo = await _todoItemRepository.GetByIdAsync(todoId);
        if (todo != null)
        {
            todo.IsDone = !todo.IsDone;
            await _todoItemRepository.UpdateAsync(todo);
        }
    }

    public async Task UpdateTodoAsync(TodoItem todoItem)
    {
        await _todoItemRepository.UpdateAsync(todoItem);
    }

    public async Task DeleteTodoAsync(Guid taskId, Guid todoId)
    {
        await _todoItemRepository.DeleteAsync(todoId);
    }

    public async Task UpdateTaskNotesAsync(Guid taskId, string notes)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null)
        {
            task.Notes = notes;
            await _taskRepository.UpdateAsync(task);
        }
    }
}
