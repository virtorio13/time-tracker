using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces;

public interface ITodoItemRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id);
    Task AddAsync(TodoItem todoItem);
    Task UpdateAsync(TodoItem todoItem);
    Task DeleteAsync(Guid id);
}
