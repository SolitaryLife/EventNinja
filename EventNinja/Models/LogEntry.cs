using System;

namespace EventNinja.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? MethodName { get; set; }
        public string? ClassName { get; set; }

        public override string ToString()
        {
            var logLine = $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] [{Category}]";
            
            if (!string.IsNullOrEmpty(ClassName) && !string.IsNullOrEmpty(MethodName))
            {
                logLine += $" [{ClassName}.{MethodName}]";
            }
            
            logLine += $" {Message}";
            
            if (!string.IsNullOrEmpty(Exception))
            {
                logLine += $" | Exception: {Exception}";
            }
            
            return logLine;
        }
    }
}