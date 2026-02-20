using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;
using TimeTracker.Data;
using TimeTracker.Data.Repositories;
using TimeTracker.UI.ViewModels;
using TimeTracker.UI.Views;

namespace TimeTracker.UI;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validation errors in Avalonia.
        BindingPlugins.DataValidators.RemoveAt(0);

        var collection = new ServiceCollection();
        ConfigureServices(collection);
        Services = collection.BuildServiceProvider();

        // Ensure database is created
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TimeTrackerDbContext>();
            context.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            var vm = Services.GetRequiredService<MainWindowViewModel>();
            
            vm.ShowSaveFileDialog = async () =>
            {
                var file = await mainWindow.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title = "Export CSV",
                    DefaultExtension = "csv",
                    FileTypeChoices = new[]
                    {
                        new Avalonia.Platform.Storage.FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
                    }
                });

                return file?.Path.LocalPath;
            };

            mainWindow.DataContext = vm;

            vm.ShowSummaryWindow = () =>
            {
                var summaryVm = new SummaryViewModel(Services.GetRequiredService<ISummaryService>());
                var summaryWindow = new SummaryWindow
                {
                    DataContext = summaryVm
                };
                summaryWindow.Show(mainWindow);
            };

            vm.ShowConfirmDialog = async (title, message) =>
            {
                var dialog = new ConfirmDialog(title, message);
                return await dialog.ShowDialog<bool>(mainWindow);
            };

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = System.IO.Path.Combine(folder, "TimeTracker");
        System.IO.Directory.CreateDirectory(path);
        var dbPath = System.IO.Path.Combine(path, "timetracker.db");
        
        services.AddDbContext<TimeTrackerDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<ITodoItemRepository, TodoItemRepository>();

        // Services
        services.AddTransient<ITimeTrackingService, TimeTrackingService>();
        services.AddTransient<ITaskService, TaskService>();
        services.AddTransient<IExportService, CsvExportService>();
        services.AddTransient<ISummaryService, SummaryService>();

        services.AddTransient<MainWindowViewModel>();
    }
}