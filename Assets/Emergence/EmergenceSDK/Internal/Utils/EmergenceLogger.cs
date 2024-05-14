// Comment in the following define to disable logging
//#define DISABLE_EMERGENCE_LOGS

using System;
using System.Diagnostics;
using System.Text;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Types;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Internal.Utils
{
    /// <summary>
    /// Error logger, used to log HTTP errors and other messages to the console or a file.
    /// </summary>
    public static class EmergenceLogger 
    {
#if DISABLE_EMERGENCE_LOGS
        internal static IDisposable VerboseOutput => null;
        internal static IDisposable VerboseMarker => null;
#else
        /// <summary>
        /// You MUST dispose the IDisposable created by this property, recommended with the using keyword.
        /// It will let the <see cref="EmergenceLogger"/> also output verbose logs.
        /// <remarks>Forgetting to <see cref="VerboseOutputManager.Dispose"/> the <see cref="VerboseOutputManager"/> will emit a warning and automatically dispose it.</remarks>
        /// </summary>
        internal static IDisposable VerboseOutput(bool enabled) => new VerboseOutputManager(enabled);
        
        /// <summary>
        /// You MUST dispose the IDisposable created by this property, recommended with the using keyword.
        /// It will mark any future logs logged by the <see cref="EmergenceLogger"/> as verbose logs.
        /// <remarks>Forgetting to <see cref="MarkLogsAsVerboseManager.Dispose"/> the <see cref="MarkLogsAsVerboseManager"/> will emit a warning and automatically dispose it.</remarks>
        /// </summary>
        internal static IDisposable VerboseMarker(bool enabled) => new MarkLogsAsVerboseManager(enabled);
#endif

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
        private static readonly LogLevel MaxLogLevel = EmergenceSingleton.Instance.LogLevel;
        
        public static void LogWarning(string message, bool alsoLogToScreen = false)
        {
            if (IsEnabledFor(LogLevel.Warning))
            {
                LogWithoutCode(message, LogLevel.Warning, alsoLogToScreen);
            }
        }
        
        public static void LogWarning(Exception exception, string prefix = "")
        {
            LogException(exception, LogWarning, prefix);
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
        
        public static void LogError(Exception exception, string prefix = "")
        {
            LogException(exception, LogError, prefix);
        }
        public static void LogInfo(string message, bool alsoLogToScreen = false)
        {
            if (IsEnabledFor(LogLevel.Info))
            {
                LogWithoutCode(message, LogLevel.Info, alsoLogToScreen);
            }
        }

        public static void LogInfo(Exception exception, string prefix = "")
        {
            LogException(exception, LogInfo, prefix);
        }

        public static void LogWebResponse(WebResponse response)
        {
#if DISABLE_EMERGENCE_LOGS
            return; // Early return to avoid any logic
#endif
            if ((MarkLogsAsVerbose && !VerboseMode) || response == null)
                return; // Early return to avoid any logic

            var info = WebRequestService.Instance.GetRequestInfoByWebResponse(response);
            var requestId = (info?.Id)?.ToString() ?? "UNKNOWN";
            var message = $"Request #{requestId}: Completed with code {response.StatusCode}";
            string requestHeaders = "";
            string responseHeaders = "";
            
            if (VerboseMode) {GetHeaders();}
            
            if (response is FailedWebResponse { Exception: not OperationCanceledException and not TimeoutException } failedResponse)
            {
                LogError(message);
                LogError($"Request #{requestId}: {response.Error}");
                if (failedResponse.Exception != null)
                    LogError(failedResponse.Exception, $"Request #{requestId}: ");
                
                using (VerboseMarker(true))
                {
                    LogError($"Request #{requestId}: Request headers:{Environment.NewLine}{requestHeaders}");
                    LogError($"Request #{requestId}: Request body:{Environment.NewLine}{Encoding.UTF8.GetString(response.ResponseBytes)}");
                    LogError($"Request #{requestId}: Response headers:{Environment.NewLine}{responseHeaders}");
                    LogError($"Request #{requestId}: Response body:{Environment.NewLine}{response.ResponseText}");
                }
            }
            else if (response is FailedWebResponse { Exception: OperationCanceledException } canceledResponse)
            {
                LogInfo($"Request #{requestId}: Request cancelled");
                LogInfo(canceledResponse.Exception, $"Request #{requestId}: ");
                
                using (VerboseMarker(true))
                {
                    LogInfo($"Request #{requestId}: Request headers:{Environment.NewLine}{requestHeaders}");
                    LogInfo($"Request #{requestId}: Request body:{Environment.NewLine}{Encoding.UTF8.GetString(response.ResponseBytes)}");
                }
            }
            else if (response is FailedWebResponse { Exception: TimeoutException } timeoutResponse)
            {
                LogError($"Request #{requestId}: Request timed out");
                LogError(timeoutResponse.Exception, $"Request #{requestId}: ");
                
                using (VerboseMarker(true))
                {
                    LogError($"Request #{requestId}: Request headers:{Environment.NewLine}{requestHeaders}");
                    LogError($"Request #{requestId}: Request body:{Environment.NewLine}{Encoding.UTF8.GetString(response.ResponseBytes)}");
                }
            }
            else
            {
                if ((int)response.StatusCode is >= 200 and < 300)
                {
                    LogInfo(message);
                    
                    using (VerboseMarker(true))
                    {
                        LogInfo($"Request #{requestId}: Request headers:{Environment.NewLine}{requestHeaders}");
                        LogInfo($"Request #{requestId}: Request body:{Environment.NewLine}{Encoding.UTF8.GetString(response.ResponseBytes)}");
                        LogInfo($"Request #{requestId}: Response headers:{Environment.NewLine}{responseHeaders}");
                        LogInfo($"Request #{requestId}: Response body:{Environment.NewLine}{response.ResponseText}");
                    }
                }
                else
                {
                    LogWarning(message);
                    
                    using (VerboseMarker(true))
                    {
                        LogWarning($"Request #{requestId}: Request headers:{Environment.NewLine}{requestHeaders}");
                        LogWarning($"Request #{requestId}: Request body:{Environment.NewLine}{Encoding.UTF8.GetString(response.ResponseBytes)}");
                        LogWarning($"Request #{requestId}: Response headers:{Environment.NewLine}{responseHeaders}");
                        LogWarning($"Request #{requestId}: Response body:{Environment.NewLine}{response.ResponseText}");
                    }
                }
            }
            
            void GetHeaders()
            {
                requestHeaders = "";
                if (info != null)
                {
                    foreach (var headerPair in info.Headers)
                    {
                        requestHeaders += $"{headerPair.Key}: {headerPair.Value}{Environment.NewLine}";
                    }
                }
                responseHeaders = "";
                foreach (var headerPair in response.Headers)
                {
                    responseHeaders += $"{headerPair.Key}: {headerPair.Value}{Environment.NewLine}";
                }
            }
        }

        private static bool VerboseMode { get; set; }
        private static bool MarkLogsAsVerbose { get; set; }
        
        private static void LogException(Exception exception, Action<string, bool> logFunction, string prefix = "")
        {
            if (MarkLogsAsVerbose && !VerboseMode)
                return;

            Exception currentException = exception;
            var level = 0;
            while (currentException != null) {
                logFunction(new string('\t', level) + prefix + exception.GetType().FullName + ": " + exception.Message, false);
                currentException = currentException.InnerException;
                level++;
            }
        }
        
        private static bool IsEnabledFor(LogLevel logLevelIn)
        {
#if DISABLE_EMERGENCE_LOGS
            return false;
#endif
            return logLevelIn <= MaxLogLevel;
        }

        private static void LogWithCode(string message, long errorCode, LogLevel logLevelIn, bool alsoLogToScreen) 
        {
#if DISABLE_EMERGENCE_LOGS
            return;
#endif
            if (MarkLogsAsVerbose && !VerboseMode)
                return;
            
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
            if (MarkLogsAsVerbose && !VerboseMode)
                return;
            
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
        
        private class VerboseOutputManager : FlagLifecycleManager<bool>
        {
            public VerboseOutputManager(bool newValue) : base(newValue) { }
            protected override bool GetCurrentFlag1Value() => VerboseMode;
            protected override void SetFlag1Value(bool newValue) => VerboseMode = newValue;
        }
        
        private class MarkLogsAsVerboseManager : FlagLifecycleManager<bool>
        {
            public MarkLogsAsVerboseManager(bool newValue) : base(newValue) { }
            protected override bool GetCurrentFlag1Value() => MarkLogsAsVerbose;
            protected override void SetFlag1Value(bool newValue) => MarkLogsAsVerbose = newValue;
        }
    }
}
