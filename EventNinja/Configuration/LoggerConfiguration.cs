using System;
using System.IO;

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
        
        public bool? EnableDebug { get; set; } = null;
        public bool? EnableInfo { get; set; } = null;
        public bool? EnableWarning { get; set; } = null;
        public bool? EnableError { get; set; } = null;
        public bool? EnableCritical { get; set; } = null;
        
        private static readonly Lazy<bool> _isDevelopment = new Lazy<bool>(() => DetectDevelopmentEnvironment());
        
        public bool IsDebugEnabled => EnableDebug ?? GetDefaultLogLevel(nameof(EnableDebug));
        public bool IsInfoEnabled => EnableInfo ?? GetDefaultLogLevel(nameof(EnableInfo));
        public bool IsWarningEnabled => EnableWarning ?? GetDefaultLogLevel(nameof(EnableWarning));
        public bool IsErrorEnabled => EnableError ?? GetDefaultLogLevel(nameof(EnableError));
        public bool IsCriticalEnabled => EnableCritical ?? GetDefaultLogLevel(nameof(EnableCritical));

        internal long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
        
        internal string GetLogDirectory()
        {
            return Path.IsPathRooted(LogDirectory) 
                ? LogDirectory 
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogDirectory);
        }
        
        private static bool DetectDevelopmentEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                           ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                           ?? "Production";
            
            return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }
        
        private bool GetDefaultLogLevel(string levelName)
        {
            var isDevelopment = _isDevelopment.Value;
            
            return levelName switch
            {
                nameof(EnableDebug) => isDevelopment,
                nameof(EnableInfo) => isDevelopment,
                nameof(EnableWarning) => true,
                nameof(EnableError) => true,
                nameof(EnableCritical) => true,
                _ => true
            };
        }
    }
}