using UnityEngine;
using System.Collections.Generic;

namespace GeoGame3D.Utils
{
    /// <summary>
    /// Simple logging framework with domain-based filtering and log levels.
    /// Can be enhanced later with more advanced frameworks like NLog or Serilog.
    /// </summary>
    public static class SimpleLogger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            None = 999
        }

        // Global minimum log level
        public static LogLevel MinimumLogLevel = LogLevel.Info;

        // Domain-specific log levels (overrides global minimum)
        private static Dictionary<string, LogLevel> domainLogLevels = new Dictionary<string, LogLevel>();

        /// <summary>
        /// Set the log level for a specific domain
        /// </summary>
        public static void SetDomainLogLevel(string domain, LogLevel level)
        {
            domainLogLevels[domain] = level;
        }

        /// <summary>
        /// Clear domain-specific log level (will use global minimum)
        /// </summary>
        public static void ClearDomainLogLevel(string domain)
        {
            domainLogLevels.Remove(domain);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void Debug(string domain, string message)
        {
            Log(domain, LogLevel.Debug, message);
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        public static void Info(string domain, string message)
        {
            Log(domain, LogLevel.Info, message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void Warning(string domain, string message)
        {
            Log(domain, LogLevel.Warning, message);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void Error(string domain, string message)
        {
            Log(domain, LogLevel.Error, message);
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private static void Log(string domain, LogLevel level, string message)
        {
            // Get the effective log level for this domain
            LogLevel effectiveLevel = MinimumLogLevel;
            if (domainLogLevels.TryGetValue(domain, out LogLevel domainLevel))
            {
                effectiveLevel = domainLevel;
            }

            // Skip if below minimum level
            if (level < effectiveLevel)
            {
                return;
            }

            // Format message with domain and level
            string formattedMessage = $"[{domain}] [{level}] {message}";

            // Use appropriate Unity log method based on level
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }
    }
}
