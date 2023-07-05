// Comment in the following define to disable logging
//#define DISABLE_EMERGENCE_LOGS

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
        private enum LogLevel
        {
            Off = 0,
            Error = 1,
            Warning = 2,
            Info = 3
        }

        /// <summary>
        /// Change this to change Emergence Logging level
        /// </summary>
        private static readonly LogLevel logLevel = LogLevel.Info;
        

        public static void LogWarning(string message)
        {
            if (IsEnabledFor(LogLevel.Warning))
            {
                LogWithoutCode(message, LogLevel.Warning);
            }
        }
        
        /// <remarks>This may log a warning depending on the code.</remarks>
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

        private static bool IsEnabledFor(LogLevel logLevelIn)
        {
            return logLevelIn <= EmergenceLogger.logLevel;
        }

        private static void LogWithCode(string message, long errorCode, LogLevel logLevelIn) 
        {
#if DISABLE_EMERGENCE_LOGS
            return;
#endif
            string callingClass = "";
            StackTrace trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames != null && frames.Length >= 3) // Get the class name from the caller's caller
            {
                callingClass = frames[2].GetMethod().DeclaringType?.FullName;
            }
            Debug.LogWarning($"{errorCode} Warning in {callingClass}: {message}");
        }

        private static void LogWithoutCode(string message, LogLevel logLevelIn)
        {
#if DISABLE_EMERGENCE_LOGS
            return;
#endif
            string callingClass = "";
            StackTrace trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames != null && frames.Length >= 3) // Get the class name from the caller's caller
            {
                callingClass = frames[2].GetMethod().DeclaringType?.FullName;
            }

            switch (logLevelIn)
            {
                case LogLevel.Info:
                    Debug.Log($"{callingClass}: {message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"{callingClass}: {message}");
                    break;
                case LogLevel.Error:
                    Debug.LogWarning($"{callingClass}: {message}");
                    break;
            }
        }
    }
}
