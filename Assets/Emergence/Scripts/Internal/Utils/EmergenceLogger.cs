// Disable the following define to disable logging
#define ENABLE_LOGS

using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Internal.Utils
{
    /// <summary>
    /// Error logger, used to log HTTP errors and other messages to the console or a file.
    /// </summary>
    public static class EmergenceLogger 
    {
        public enum LogLevel
        {
            Off = 0,
            Error = 1,
            Warning = 2,
            Info = 3
        }

        /// <summary>
        /// Change this to toggle Emergence Logging
        /// </summary>
        private static readonly LogLevel logLevel = LogLevel.Info;

        public static void LogWarning(string warning, long errorCode)
        {
            if (IsEnabledFor(LogLevel.Warning))
            {
                LogWithCode(warning, errorCode, LogLevel.Warning);
            }
        }

        public static void LogWarning(string message)
        {
            if (IsEnabledFor(LogLevel.Warning))
            {
                LogWithoutCode(message, LogLevel.Warning);
            }
        }
        
        public static void LogError(string error, long errorCode)
        {
            if (IsEnabledFor(LogLevel.Error))
            {
                LogWithCode(error, errorCode, LogLevel.Error);
            }
        }

        public static void LogError(string message)
        {
            if (IsEnabledFor(LogLevel.Error))
            {
                LogWithoutCode(message, LogLevel.Error);
            }
        }
        
        public static void LogInfo(string message)
        {
            if (IsEnabledFor(LogLevel.Info))
            {
                LogWithoutCode(message, LogLevel.Info);
            }
        }

        private static bool IsEnabledFor(LogLevel logLevel)
        {
            return logLevel <= EmergenceLogger.logLevel;
        }

        [Conditional("ENABLE_LOGS")]
        private static void LogWithCode(string message, long errorCode, LogLevel logLevel) 
        {
            bool isError = (logLevel == LogLevel.Error);
            switch (errorCode) 
            {
                case 408:
                case 502:
                case 503:
                case 504:
                    isError = false;
                    break;
            }

            string callingClass = "";
            StackTrace trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames.Length >= 3) // Get the class name from the caller's caller
            {
                callingClass = frames[2].GetMethod().DeclaringType.FullName;
            }

            if (isError)
            {
                EmergenceLogger.LogError($"{errorCode} Error in {callingClass}: {message}");
            }
            else
            {
                EmergenceLogger.LogWarning($"{errorCode} Warning in {callingClass}: {message}");
            }
        }

        [Conditional("ENABLE_LOGS")]
        private static void LogWithoutCode(string message, LogLevel logLevel)
        {
            string callingClass = "";
            StackTrace trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames.Length >= 3) // Get the class name from the caller's caller
            {
                callingClass = frames[2].GetMethod().DeclaringType.FullName;
            }

            switch (logLevel)
            {
                case LogLevel.Info:
                    EmergenceLogger.LogInfo($"{callingClass}: {message}");
                    break;
                case LogLevel.Warning:
                    EmergenceLogger.LogWarning($"{callingClass}: {message}");
                    break;
                case LogLevel.Error:
                    EmergenceLogger.LogError($"{callingClass}: {message}");
                    break;
            }
        }
    }
}
