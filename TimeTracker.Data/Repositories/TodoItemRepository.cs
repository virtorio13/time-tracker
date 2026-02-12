using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Data.Repositories;

public class TodoItemRepository : ITodoItemRepository
{
    private readonly TimeTrackerDbContext _context;

    public TodoItemRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id)
    {
        return await _context.TodoItems.FindAsync(id);
    }

    public async Task AddAsync(TodoItem todoItem)
    {
        await _context.TodoItems.AddAsync(todoItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TodoItem todoItem)
    {
         if (_context.Entry(todoItem).State == EntityState.Detached)
        {
            _context.TodoItems.Update(todoItem);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            _context.TodoItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
