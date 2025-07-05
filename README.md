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
- 🎛️ **Smart Log Level Control** - Auto-detects environment with customizable overrides

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
        
        // Log levels auto-detected by environment
        // Development: Debug=true, Info=true, Warning=true, Error=true, Critical=true
        // Production: Debug=false, Info=false, Warning=true, Error=true, Critical=true
        
        // Optional: Override specific levels if needed
        // config.EnableDebug = false;  // Custom override
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
| `AppName` | string | "Application" | Application name (used in filename) |
| `LogDirectory` | string | "Logs" | Base directory for log files |
| `MaxFileSizeMB` | int | 5 | Maximum file size before rotation |
| `DeleteOldLogs` | bool | false | Enable automatic cleanup |
| `RetentionDays` | int | 7 | Days to keep log files |
| `CleanupIntervalHours` | int | 24 | Hours between cleanup runs |
| `EnableDebug` | bool? | null | Override Debug level (null = auto-detect) |
| `EnableInfo` | bool? | null | Override Info level (null = auto-detect) |
| `EnableWarning` | bool? | null | Override Warning level (null = auto-detect) |
| `EnableError` | bool? | null | Override Error level (null = auto-detect) |
| `EnableCritical` | bool? | null | Override Critical level (null = auto-detect) |

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

### Smart Environment Detection

EventNinja automatically detects your environment and applies appropriate log levels:

**Environment Detection Sources:**
1. `ASPNETCORE_ENVIRONMENT` environment variable
2. `DOTNET_ENVIRONMENT` environment variable  
3. Defaults to "Production" if not found

**Default Log Level Presets:**

| Environment | Debug | Info | Warning | Error | Critical |
|-------------|-------|------|---------|-------|----------|
| Development | ✅ | ✅ | ✅ | ✅ | ✅ |
| Production | ❌ | ❌ | ✅ | ✅ | ✅ |

```csharp
// Auto-detection (recommended)
services.AddEventNinja(config =>
{
    config.AppName = "MyApp";
    // Log levels automatically set based on environment
});

// Custom overrides when needed
services.AddEventNinja(config =>
{
    config.AppName = "MyApp";
    config.EnableDebug = false;    // Force disable debug even in Development
    config.EnableInfo = true;      // Force enable info even in Production
    // Other levels use auto-detection
});
```

### Understanding Custom Override Behavior

**Important:** Custom overrides always take precedence over environment detection, regardless of whether you're in Development or Production.

**Rule:** 
- **Has Custom Value** → Uses your custom setting (ignores environment)
- **No Custom Value (null)** → Uses auto-detection based on environment

**Example Scenario:**

```csharp
services.AddEventNinja(config =>
{
    config.AppName = "MyApp";
    config.EnableDebug = false;    // Custom Override
    config.EnableInfo = true;      // Custom Override  
    // EnableWarning = null (auto-detect)
    // EnableError = null (auto-detect)
    // EnableCritical = null (auto-detect)
});
```

**Result Table:**

| Environment | Debug | Info | Warning | Error | Critical |
|-------------|-------|------|---------|-------|----------|
| **Development** | ❌ (custom) | ✅ (custom) | ✅ (auto) | ✅ (auto) | ✅ (auto) |
| **Production** | ❌ (custom) | ✅ (custom) | ✅ (auto) | ✅ (auto) | ✅ (auto) |

**Key Points:**
1. `EnableDebug = false` applies to **both** Development and Production
2. `EnableInfo = true` applies to **both** Development and Production  
3. Warning, Error, Critical use environment-based defaults
4. Custom overrides are **environment-independent**

**When to Use Custom Overrides:**
- **Disable Debug in Development:** When debug logs are too noisy during testing
- **Enable Info in Production:** When you need specific info logs in production for monitoring
- **Temporary Debugging:** Enable debug logs in production for troubleshooting (not recommended for long-term)

**Best Practice:**
```csharp
// Recommended: Let auto-detection handle most cases
services.AddEventNinja(config =>
{
    config.AppName = "MyApp";
    // Only override when you have specific requirements
    // config.EnableDebug = false;  // Uncomment only if needed
});
```

## 📄 License

Copyright © 2025 Tofu Survivors

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ for the .NET community**
