using System;
using System.Runtime.CompilerServices;
using EventNinja.Models;

namespace EventNinja.Services
{
    public interface ILoggerService
    {
        void LogDebug(string message, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "");
            
        void LogInfo(string message, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "");
            
        void LogWarning(string message, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "");
            
        void LogError(string message, Exception? exception = null, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "");
            
        void LogCritical(string message, Exception? exception = null, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "");
            
        void Log(LogLevel level, string message, Exception? exception = null, string category = "Application", 
            [CallerMemberName] string methodName = "", 
            [CallerFilePath] string className = "");
    }
}