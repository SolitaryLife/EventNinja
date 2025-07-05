using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventNinja.Configuration;
using EventNinja.Models;

namespace EventNinja.Services
{
    public class LoggerService : ILoggerService, IDisposable
    {
        private readonly LoggerConfiguration _config;
        private readonly string _logDirectory;
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _writerTask;
        private readonly Task _cleanupTask;
        private DateTime _lastCleanupTime;

        public LoggerService(LoggerConfiguration configuration)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logDirectory = _config.GetLogDirectory();
            _logQueue = new ConcurrentQueue<LogEntry>();
            _cancellationTokenSource = new CancellationTokenSource();
            _lastCleanupTime = DateTime.Now;
            
            EnsureLogDirectoryExists();
            
            // เริ่ม background tasks
            _writerTask = Task.Run(ProcessLogQueue, _cancellationTokenSource.Token);
            _cleanupTask = Task.Run(ProcessCleanup, _cancellationTokenSource.Token);
        }

        private void EnsureLogDirectoryExists()
        {
            var fullLogPath = GetCurrentLogDirectoryPath();
            if (!Directory.Exists(fullLogPath))
            {
                Directory.CreateDirectory(fullLogPath);
            }
        }

        private string GetCurrentLogDirectoryPath()
        {
            var now = DateTime.Now;
            var monthName = now.ToString("MMMM", System.Globalization.CultureInfo.CurrentCulture);
            var yearMonthPath = Path.Combine(now.Year.ToString(), monthName);
            return Path.Combine(_logDirectory, yearMonthPath);
        }

        public void LogDebug(string message, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "")
        {
            Log(LogLevel.Debug, message, null, category, methodName, className);
        }

        public void LogInfo(string message, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "")
        {
            Log(LogLevel.Info, message, null, category, methodName, className);
        }

        public void LogWarning(string message, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "")
        {
            Log(LogLevel.Warning, message, null, category, methodName, className);
        }

        public void LogError(string message, Exception? exception = null, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "")
        {
            Log(LogLevel.Error, message, exception, category, methodName, className);
        }

        public void LogCritical(string message, Exception? exception = null, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "")
        {
            Log(LogLevel.Critical, message, exception, category, methodName, className);
        }

        public void Log(LogLevel level, string message, Exception? exception = null, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "")
        {
            // ตรวจสอบว่า log level นี้ถูก enable หรือไม่
            if (!IsLogLevelEnabled(level))
            {
                return;
            }

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Category = category,
                Message = message,
                Exception = exception?.ToString(),
                MethodName = methodName,
                ClassName = Path.GetFileNameWithoutExtension(className)
            };

            // เพิ่มลง queue
            _logQueue.Enqueue(logEntry);
        }

        private async Task ProcessLogQueue()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (_logQueue.TryDequeue(out var logEntry))
                    {
                        await WriteToFileAsync(logEntry);
                    }
                    else
                    {
                        // รอสักครู่ก่อนเช็คใหม่
                        await Task.Delay(100, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Silent fail
                }
            }

            // เขียน log ที่เหลือใน queue
            while (_logQueue.TryDequeue(out var logEntry))
            {
                try
                {
                    await WriteToFileAsync(logEntry);
                }
                catch
                {
                    // Silent fail
                }
            }
        }

        private async Task WriteToFileAsync(LogEntry logEntry)
        {
            // อัปเดต log directory สำหรับเดือนปัจจุบัน
            EnsureLogDirectoryExists();
            
            var logFileName = GetCurrentLogFileName();
            var logDirectoryPath = GetCurrentLogDirectoryPath();
            var logFilePath = Path.Combine(logDirectoryPath, logFileName);
            
            // ตรวจสอบขนาดไฟล์ก่อนเขียน
            if (File.Exists(logFilePath))
            {
                var fileInfo = new FileInfo(logFilePath);
                if (fileInfo.Length >= _config.MaxFileSizeBytes)
                {
                    RotateLogFile(logFilePath);
                }
            }
            
            // เขียน log ลงไฟล์แบบ async
            await File.AppendAllTextAsync(logFilePath, logEntry.ToString() + Environment.NewLine);
        }

        private string GetCurrentLogFileName()
        {
            return $"{_config.AppName}_{DateTime.Now:yyyyMMdd}.log";
        }

        private void RotateLogFile(string currentLogPath)
        {
            try
            {
                var baseFileName = Path.GetFileNameWithoutExtension(currentLogPath);
                var directory = Path.GetDirectoryName(currentLogPath);
                
                // หาหมายเลข backup ถัดไป
                int nextBackupNumber = GetNextBackupNumber(directory!, baseFileName);
                
                // ย้ายไฟล์ปัจจุบันเป็น backup
                var backupPath = Path.Combine(directory!, $"{baseFileName}.log{nextBackupNumber}");
                if (File.Exists(currentLogPath))
                {
                    File.Move(currentLogPath, backupPath);
                }
            }
            catch
            {
                // Silent fail - ถ้า rotate ไม่ได้ก็ยังเขียน log ต่อได้
            }
        }

        private int GetNextBackupNumber(string directory, string baseFileName)
        {
            int maxNumber = 0;
            var logFiles = Directory.GetFiles(directory, $"{baseFileName}.log*");
            
            foreach (var file in logFiles)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith($"{baseFileName}.log") && fileName != $"{baseFileName}.log")
                {
                    var numberPart = fileName.Substring($"{baseFileName}.log".Length);
                    if (int.TryParse(numberPart, out int number))
                    {
                        maxNumber = Math.Max(maxNumber, number);
                    }
                }
            }
            
            return maxNumber + 1;
        }

        private async Task ProcessCleanup()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (_config.DeleteOldLogs && DateTime.Now.Subtract(_lastCleanupTime).TotalHours >= _config.CleanupIntervalHours)
                    {
                        CleanupOldLogFiles();
                        _lastCleanupTime = DateTime.Now;
                    }

                    // รอ 1 ชั่วโมงก่อนเช็คใหม่
                    await Task.Delay(TimeSpan.FromHours(1), _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Silent fail
                }
            }
        }

        private void CleanupOldLogFiles()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);
                
                // ค้นหาไฟล์ log ในทุก subdirectory (ปี/เดือน)
                var logFiles = Directory.GetFiles(_logDirectory, "*.log*", SearchOption.AllDirectories);

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(logFile);
                    }
                }
                
                // ลบ directory เปล่า (ถ้ามี)
                CleanupEmptyDirectories(_logDirectory);
            }
            catch
            {
                // Silent fail
            }
        }

        private bool IsLogLevelEnabled(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => _config.IsDebugEnabled,
                LogLevel.Info => _config.IsInfoEnabled,
                LogLevel.Warning => _config.IsWarningEnabled,
                LogLevel.Error => _config.IsErrorEnabled,
                LogLevel.Critical => _config.IsCriticalEnabled,
                _ => true
            };
        }

        private void CleanupEmptyDirectories(string directory)
        {
            try
            {
                foreach (var subDirectory in Directory.GetDirectories(directory))
                {
                    CleanupEmptyDirectories(subDirectory);
                    
                    // ลบ directory ถ้าไม่มีไฟล์และไม่มี subdirectory
                    if (!Directory.EnumerateFileSystemEntries(subDirectory).Any())
                    {
                        Directory.Delete(subDirectory);
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            
            try
            {
                Task.WaitAll(new[] { _writerTask, _cleanupTask }, TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Silent fail
            }
            
            _cancellationTokenSource.Dispose();
        }
    }
}