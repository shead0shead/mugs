// Mugs/Services/LoggerService.cs

namespace Mugs.Services
{
    public static class LoggerService
    {
        private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, $"mugs_{DateTime.Now:yyyyMMdd}.log");
        private static readonly object _lock = new object();

        static LoggerService()
        {
            Directory.CreateDirectory(LogDirectory);
        }

        public static void Log(LogLevel level, string message, Exception exception = null)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";

            if (exception != null)
            {
                logEntry += $"\nException: {exception}\nStack Trace: {exception.StackTrace}";
            }

            lock (_lock)
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
        }

        public static async Task LogAsync(LogLevel level, string message, Exception exception = null)
        {
            await Task.Run(() => Log(level, message, exception));
        }

        public static void LogDebug(string message) => Log(LogLevel.Debug, message);
        public static void LogInfo(string message) => Log(LogLevel.Info, message);
        public static void LogWarning(string message) => Log(LogLevel.Warning, message);
        public static void LogError(string message, Exception ex = null) => Log(LogLevel.Error, message, ex);
        public static void LogCritical(string message, Exception ex = null) => Log(LogLevel.Critical, message, ex);
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}