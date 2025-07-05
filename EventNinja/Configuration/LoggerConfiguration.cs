using System;

namespace EventNinja.Configuration
{
    public class LoggerConfiguration
    {
        public string AppName { get; set; } = "Application";
        public string LogDirectory { get; set; } = "Logs";
        public int MaxFileSizeMB { get; set; } = 5;
        public bool DeleteOldLogs { get; set; } = false;
        public int RetentionDays { get; set; } = 7;
        public int CleanupIntervalHours { get; set; } = 24;
        
        public bool EnableDebug { get; set; } = true;
        public bool EnableInfo { get; set; } = true;
        public bool EnableWarning { get; set; } = true;
        public bool EnableError { get; set; } = true;
        public bool EnableCritical { get; set; } = true;

        internal long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
        
        internal string GetLogDirectory()
        {
            return Path.IsPathRooted(LogDirectory) 
                ? LogDirectory 
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogDirectory);
        }
    }
}