# Time Tracker

A cross-platform Time Tracking and Todo application built with C# .NET 9 and Avalonia UI.

## Features

- **Task Management**: Create and organize tasks with support for hierarchical sub-tasks.
- **Time Tracking**: Track and record time spent on specific tasks.
- **Todo Lists**: Manage todo items associated with each task.
- **Data Persistence**: Robust data storage using SQLite with Entity Framework Core.
- **Visualizations**: Visualize time distribution with interactive charts.
- **Cross-Platform**: Designed to run on Windows, macOS, and Linux.

## Technologies Used

- **Framework**: [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- **UI Framework**: [Avalonia UI](https://avaloniaui.net/)
- **Database**: [SQLite](https://www.sqlite.org/index.html) via [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- **MVVM Toolkit**: [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- **Charting**: [LiveCharts2](https://livecharts.dev/)

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd time-tracker
   ```

2. Restore dependencies and build the project:
   ```bash
   dotnet build
   ```

### Running the Application

You can run the application directly from the command line:

```bash
dotnet run --project TimeTracker.UI
```

## Database Location

The application stores data in a local SQLite database file named `timetracker.db`. The location varies by operating system:

- **Windows**: `%LOCALAPPDATA%\TimeTracker\timetracker.db`
- **macOS**: `~/Library/Application Support/TimeTracker/timetracker.db`
- **Linux**: `~/.local/share/TimeTracker/timetracker.db`

## Project Structure

- **TimeTracker.UI**: Main application project containing the Avalonia UI views and view models.
- **TimeTracker.Core**: Core domain logic, including entities (`TrackedTask`, `TimeEntry`, `TodoItem`) and interfaces.
- **TimeTracker.Data**: Data access layer handling database context and migrations.
- **TimeTracker.Verification**: Unit and testing project.
