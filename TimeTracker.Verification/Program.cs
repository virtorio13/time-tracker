using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;
using TimeTracker.Data;
using TimeTracker.Data.Repositories;

// Setup DI
var services = new ServiceCollection();
services.AddDbContext<TimeTrackerDbContext>(options =>
    options.UseSqlite("Data Source=timetracker_test.db"));

services.AddScoped<ITaskRepository, TaskRepository>();
services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
services.AddScoped<ITodoItemRepository, TodoItemRepository>();
services.AddScoped<ITimeTrackingService, TimeTrackingService>();
services.AddScoped<ITaskService, TaskService>();
services.AddScoped<IExportService, CsvExportService>();

var provider = services.BuildServiceProvider();

// Ensure clean DB
using (var scope = provider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TimeTrackerDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

var taskService = provider.GetRequiredService<ITaskService>();
var timeTrackingService = provider.GetRequiredService<ITimeTrackingService>();
var exportService = provider.GetRequiredService<IExportService>();
var entryRepo = provider.GetRequiredService<ITimeEntryRepository>();

Console.WriteLine("Starting Verification...");

// 1. Create Task
Console.WriteLine("Creating Task...");
var root = await taskService.CreateTaskAsync("Project Alpha");
Console.WriteLine($"Created Root Task: {root.Name} ({root.Id})");

// 2. Create Subtask
var sub = await taskService.CreateTaskAsync("Research", root.Id);
Console.WriteLine($"Created Subtask: {sub.Name} ({sub.Id})");

// 3. Start Task
Console.WriteLine("Starting Subtask...");
await timeTrackingService.StartTaskAsync(sub.Id);
var activeEntry = await timeTrackingService.GetActiveTimeEntryAsync(sub.Id);
if (activeEntry != null) Console.WriteLine("Task Started successfully.");
else Console.WriteLine("ERROR: Task not started.");

// 4. Wait
Console.WriteLine("Waiting 2 seconds...");
await Task.Delay(2000);

// 5. Stop Task
Console.WriteLine("Stopping Task...");
await timeTrackingService.StopTaskAsync(sub.Id);
var entries = await entryRepo.GetByTaskIdAsync(sub.Id);
var entry = entries.FirstOrDefault();
if (entry != null && entry.EndTime.HasValue) 
{
    Console.WriteLine($"Task Stopped. Duration: {entry.EndTime - entry.StartTime}");
}
else
{
    Console.WriteLine("ERROR: Task not stopped correctly.");
}

// 6. Add Todo
Console.WriteLine("Adding Todo...");
var todo = await taskService.AddTodoAsync(sub.Id, "Check Docs");
Console.WriteLine($"Todo Added: {todo.Description}");

// 7. Toggle Todo
Console.WriteLine("Toggling Todo...");
await taskService.ToggleTodoAsync(sub.Id, todo.Id);
// Verify
var tasks = await taskService.GetAllTasksAsync();
var updatedSub = tasks.First(t => t.Id == sub.Id);
var updatedTodo = updatedSub.TodoItems.First(t => t.Id == todo.Id);
Console.WriteLine($"Todo IsDone: {updatedTodo.IsDone}");

// 8. Export
Console.WriteLine("Exporting CSV...");
var path = "export_test.csv";
await exportService.ExportToCsvAsync(path);
if (File.Exists(path))
{
    Console.WriteLine("CSV Exported.");
    var content = await File.ReadAllTextAsync(path);
    Console.WriteLine("CSV Content Preview:");
    Console.WriteLine(content);
}
else
{
    Console.WriteLine("ERROR: CSV not exported.");
}


Console.WriteLine("Verification Completed.");
