// Comment in the following define to disable logging
//#define DISABLE_EMERGENCE_LOGS

using System;
using System.Diagnostics;
using EmergenceSDK.Types;
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
            Trace = 1,
            Error = 2,
            Warning = 3,
            Info = 4
        }

        /// <summary>
        /// Change this to change Emergence Logging level
        /// </summary>
        private static readonly LogLevel logLevel = EmergenceSingleton.Instance.LogLevel;
        

        public static void LogWarning(string message, bool alsoLogToScreen = false)
        {
            if (IsEnabledFor(LogLevel.Warning))
            {
                LogWithoutCode(message, LogLevel.Warning, alsoLogToScreen);
            }
        }
        
        
        public static void LogError(string error, long errorCode) => LogError(error, errorCode, false);
        public static void LogError(string error, long errorCode, bool alsoLogToScreen)
        {
            if (IsEnabledFor(LogLevel.Error))
            {
                LogWithCode(error, errorCode, LogLevel.Error, alsoLogToScreen);
            }
        }

        public static void LogError(string message, bool alsoLogToScreen = false)
        {
            if (IsEnabledFor(LogLevel.Error))
            {
                LogWithoutCode(message, LogLevel.Error, alsoLogToScreen);
            }
        }
        
        public static void LogInfo(string message, bool alsoLogToScreen = false)
        {
            if (IsEnabledFor(LogLevel.Info))
            {
                LogWithoutCode(message, LogLevel.Info, alsoLogToScreen);
            }
        }

        private static bool IsEnabledFor(LogLevel logLevelIn)
        {
            return logLevelIn <= logLevel;
        }

        private static void LogWithCode(string message, long errorCode, LogLevel logLevelIn, bool alsoLogToScreen) 
        {
#if DISABLE_EMERGENCE_LOGS
            return;
#endif
            var callingClass = CallingClass();
            Debug.LogWarning($"{errorCode} Warning in {callingClass}: {message}");
            if(alsoLogToScreen)
                OnScreenLogger.Instance.HandleLog(message, callingClass, logLevelIn);
        }


        private static void LogWithoutCode(string message, LogLevel logLevelIn, bool alsoLogToScreen)
        {
#if DISABLE_EMERGENCE_LOGS
            return;
#endif
            
            var callingClass = CallingClass();

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
            
            if(alsoLogToScreen)
                OnScreenLogger.Instance.HandleLog(message, callingClass, logLevelIn);
        }
        private static string CallingClass()
        {
            string callingClass = "";
            StackTrace trace = new StackTrace();
            StackFrame[] frames = trace.GetFrames();
            if (frames != null && frames.Length >= 4) // Get the class name from the caller's caller
            {
                callingClass = frames[3].GetMethod().DeclaringType?.FullName;
            }

            return callingClass;
        }
    }
        
}
