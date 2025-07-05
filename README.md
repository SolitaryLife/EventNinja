# ⚡ EventNinja

**A powerful, thread-safe logging library for .NET applications with automatic file management and international support.**

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Thread-Safe](https://img.shields.io/badge/thread--safe-✅-brightgreen)
![Production Ready](https://img.shields.io/badge/production--ready-✅-brightgreen)

## ✨ Features

- 🚀 **High Performance** - Non-blocking, concurrent logging
- 🔒 **Thread-Safe** - Multiple threads can log simultaneously  
- 📁 **Smart File Organization** - Automatic year/month directory structure
- 🔄 **Auto File Rotation** - Configurable file size limits with backup system
- 🧹 **Auto Cleanup** - Automatic deletion of old log files
- 🌍 **International Support** - Adapts to system culture (Thai Buddhist calendar, English, etc.)
- 🎯 **Structured Logging** - Detailed log entries with timestamp, level, category, and method info
- ⚙️ **Dependency Injection** - Full DI container support
- 📊 **Multiple Log Levels** - Debug, Info, Warning, Error, Critical
- 🎛️ **Log Level Control** - Enable/disable specific log levels for different environments

## 📦 Installation

```bash
# Clone the repository
git clone https://github.com/SolitaryLife/EventNinja.git

# Add project reference
dotnet add reference path/to/EventNinja/EventNinja.csproj
```

## 🚀 Quick Start

### 1. Configure Services (Program.cs)

```csharp
using EventNinja.Extensions;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddEventNinja(config =>
    {
        config.AppName = "MyApplication";
        config.LogDirectory = "Logs";
        config.MaxFileSizeMB = 5;
        config.DeleteOldLogs = true;
        config.RetentionDays = 30;
        config.CleanupIntervalHours = 24;
        
        // Log level control
        config.EnableDebug = true;
        config.EnableInfo = true;
        config.EnableWarning = true;
        config.EnableError = true;
        config.EnableCritical = true;
    });
});
```

### 2. Inject and Use

```csharp
public class MyService
{
    private readonly ILoggerService _logger;
    
    public MyService(ILoggerService logger)
    {
        _logger = logger;
    }
    
    public async Task DoSomething()
    {
        _logger.LogInfo("Starting operation", "MyService");
        
        try
        {
            // Your code here
            _logger.LogInfo("Operation completed successfully", "MyService");
        }
        catch (Exception ex)
        {
            _logger.LogError("Operation failed", ex, "MyService");
            throw;
        }
    }
}
```

## 📝 Log Levels

```csharp
_logger.LogDebug("Debug information");
_logger.LogInfo("General information");
_logger.LogWarning("Warning message");
_logger.LogError("Error occurred", exception);
_logger.LogCritical("Critical system failure", exception);
```

## 📁 File Organization

EventNinja automatically organizes log files by year and month according to your system's culture:

### English System (en-US)
```
Logs/
├── 2025/
│   ├── January/
│   │   ├── MyApp_20250101.log
│   │   ├── MyApp_20250102.log
│   │   └── MyApp_20250102.log1  (backup)
│   ├── July/
│   └── December/
└── 2026/
```

### Thai System (th-TH)
```
Logs/
├── 2568/
│   ├── มกราคม/
│   │   ├── MyApp_25680101.log
│   │   └── MyApp_25680102.log
│   ├── กรกฎาคม/
│   └── ธันวาคม/
└── 2569/
```

## 📊 Log Format

```
[2025-07-05 14:30:25.123] [Info] [MyService] [MyService.DoSomething] Operation completed successfully
[2025-07-05 14:30:26.456] [Warning] [Database] Connection timeout detected
[2025-07-05 14:30:27.789] [Error] [FileService] [FileService.SaveFile] Failed to save file | Exception: System.IO.IOException: Access denied
```

**Format:** `[Timestamp] [Level] [Category] [Class.Method] Message | Exception`

## ⚙️ Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `AppName` | string | Required | Application name (used in filename) |
| `LogDirectory` | string | "Logs" | Base directory for log files |
| `MaxFileSizeMB` | int | 5 | Maximum file size before rotation |
| `DeleteOldLogs` | bool | true | Enable automatic cleanup |
| `RetentionDays` | int | 30 | Days to keep log files |
| `CleanupIntervalHours` | int | 24 | Hours between cleanup runs |
| `EnableDebug` | bool | true | Enable Debug level logging |
| `EnableInfo` | bool | true | Enable Info level logging |
| `EnableWarning` | bool | true | Enable Warning level logging |
| `EnableError` | bool | true | Enable Error level logging |
| `EnableCritical` | bool | true | Enable Critical level logging |

## 🔄 File Rotation

When a log file reaches the maximum size:

1. Current file: `MyApp_20250705.log` → `MyApp_20250705.log1`
2. New current file: `MyApp_20250705.log` (created)
3. Existing backups: `.log1` → `.log2`, `.log2` → `.log3`, etc.

## 🧹 Auto Cleanup

- Runs every 24 hours (configurable)
- Deletes files older than retention period
- Removes empty directories
- Searches all subdirectories

## 🔒 Thread Safety

EventNinja is designed for high-concurrency scenarios:

- ✅ Multiple threads can log simultaneously
- ✅ Non-blocking operations
- ✅ Background file writing
- ✅ Thread-safe queue management

## 🎯 Use Cases

- **Web Applications** - Request logging, error tracking
- **Windows Services** - Service monitoring, debug info
- **Desktop Applications** - User activity, system events
- **Industrial Applications** - PLC communication, sensor data
- **Microservices** - Distributed logging, performance monitoring

## 🛠️ Advanced Usage

### Custom Log Categories

```csharp
_logger.LogInfo("User logged in", "Authentication");
_logger.LogWarning("High memory usage", "Performance");
_logger.LogError("Database connection failed", ex, "Database");
```

### Method Name Auto-Detection

```csharp
public async Task ProcessOrder(int orderId)
{
    // Automatically includes method name "ProcessOrder"
    _logger.LogInfo($"Processing order {orderId}", "OrderService");
}
```

### Environment-Based Log Level Control

```csharp
// Development Environment - Log everything
services.AddEventNinja(config =>
{
    config.AppName = "MyApp";
    config.EnableDebug = true;
    config.EnableInfo = true;
    config.EnableWarning = true;
    config.EnableError = true;
    config.EnableCritical = true;
});

// Production Environment - Only important logs
services.AddEventNinja(config =>
{
    config.AppName = "MyApp";
    config.EnableDebug = false;    // Skip debug logs
    config.EnableInfo = false;     // Skip info logs
    config.EnableWarning = true;   // Keep warnings
    config.EnableError = true;     // Keep errors
    config.EnableCritical = true;  // Keep critical logs
});
```

## 📄 License

Copyright © 2025 Tofu Survivors

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ for the .NET community**
