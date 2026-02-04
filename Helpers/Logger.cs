using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WSL_Manager.Helpers
{
    /// <summary>
    /// Service de logging structuré pour l'application
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Niveau de log
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Log un message avec le niveau Debug
        /// </summary>
        public static void Debug(string message, [CallerMemberName] string caller = "")
        {
            Log(LogLevel.Debug, message, caller);
        }

        /// <summary>
        /// Log un message avec le niveau Info
        /// </summary>
        public static void Info(string message, [CallerMemberName] string caller = "")
        {
            Log(LogLevel.Info, message, caller);
        }

        /// <summary>
        /// Log un message avec le niveau Warning
        /// </summary>
        public static void Warning(string message, [CallerMemberName] string caller = "")
        {
            Log(LogLevel.Warning, message, caller);
        }

        /// <summary>
        /// Log un message avec le niveau Error
        /// </summary>
        public static void Error(string message, [CallerMemberName] string caller = "")
        {
            Log(LogLevel.Error, message, caller);
        }

        /// <summary>
        /// Log une exception avec le niveau Error
        /// </summary>
        public static void Error(string message, Exception ex, [CallerMemberName] string caller = "")
        {
            Log(LogLevel.Error, $"{message} - Exception: {ex.Message}", caller);
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        /// <summary>
        /// Log un début d'opération
        /// </summary>
        public static void BeginOperation(string operationName, string? details = null, [CallerMemberName] string caller = "")
        {
            var message = string.IsNullOrEmpty(details)
                ? $"=== Début {operationName} ==="
                : $"=== Début {operationName}: {details} ===";
            Log(LogLevel.Debug, message, caller);
        }

        /// <summary>
        /// Log une fin d'opération
        /// </summary>
        public static void EndOperation(string operationName, bool success = true, [CallerMemberName] string caller = "")
        {
            var message = $"=== Fin {operationName} - {(success ? "Succès" : "Échec")} ===";
            Log(success ? LogLevel.Debug : LogLevel.Warning, message, caller);
        }

        /// <summary>
        /// Méthode de log générique
        /// </summary>
        private static void Log(LogLevel level, string message, string caller)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelStr = level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Info => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                _ => "UNKNOWN"
            };

            System.Diagnostics.Debug.WriteLine($"[{timestamp}] [{levelStr}] [{caller}] {message}");
        }
    }
}
