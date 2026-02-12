using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;

namespace TimeTracker.Data;

public class TimeTrackerDbContext : DbContext
{
    public DbSet<TrackedTask> Tasks { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }

    public TimeTrackerDbContext()
    {
    }

    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = System.IO.Path.Combine(folder, "TimeTracker");
            System.IO.Directory.CreateDirectory(path);
            var dbPath = System.IO.Path.Combine(path, "timetracker.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TrackedTask>()
            .HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeEntry>()
            .HasOne(te => te.TrackedTask)
            .WithMany(t => t.TimeEntries)
            .HasForeignKey(te => te.TrackedTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TodoItem>()
            .HasOne(ti => ti.TrackedTask)
            .WithMany(t => t.TodoItems)
            .HasForeignKey(ti => ti.TrackedTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
