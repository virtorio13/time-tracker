using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITaskService
{
    Task<List<TrackedTask>> GetAllTasksAsync();
    Task<TrackedTask> CreateTaskAsync(string name, Guid? parentId = null);
    Task UpdateTaskAsync(TrackedTask task);
    Task DeleteTaskAsync(Guid id);
    
    Task<TodoItem> AddTodoAsync(Guid taskId, string description);
    Task ToggleTodoAsync(Guid taskId, Guid todoId);
    Task UpdateTodoAsync(TodoItem todoItem);
    Task DeleteTodoAsync(Guid taskId, Guid todoId);
    Task UpdateTaskNotesAsync(Guid taskId, string notes);
}
