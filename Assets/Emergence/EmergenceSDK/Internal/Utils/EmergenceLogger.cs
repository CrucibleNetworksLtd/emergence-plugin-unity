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

        public static void LogWebResponse(WebResponse response, bool verbose = false)
        {
#if DISABLE_EMERGENCE_LOGS
            return; // Early return to avoid any logic
#endif
            if (response == null) { return; }

            var request = response.Request;
            var info = WebRequestService.Instance.GetRequestInfo(request);
            var requestId = (info?.Id)?.ToString() ?? "UNKNOWN";
            var message = $"Request #{requestId}: Completed with code {request.responseCode}";
            string requestHeaders = "";
            string responseHeaders = "";
            
            if (verbose) {GetHeaders();}
            
            if (response is FailedWebResponse { Exception: not OperationCanceledException and not TimeoutException } failedResponse)
            {
                LogError(message);
                LogError($"Request #{requestId}: {request.error}");
                if (failedResponse.Exception != null)
                    LogError(failedResponse.Exception, $"Request #{requestId}: ");
                
                if (verbose)
                {
                    LogError($"Request #{requestId}: Request headers:{Environment.NewLine}{Environment.NewLine}{requestHeaders}");
                    LogError($"Request #{requestId}: Request body:{Environment.NewLine}{Environment.NewLine}{Encoding.UTF8.GetString(request.uploadHandler?.data ?? new byte[] {})}");
                    LogError($"Request #{requestId}: Response headers:{Environment.NewLine}{Environment.NewLine}{responseHeaders}");
                    LogError($"Request #{requestId}: Response body:{Environment.NewLine}{Environment.NewLine}{response.ResponseText}");
                }
            }
            else if (response is FailedWebResponse { Exception: OperationCanceledException } canceledResponse)
            {
                LogInfo($"Request #{requestId}: Request cancelled");
                LogInfo(canceledResponse.Exception, $"Request #{requestId}: ");
                if (verbose)
                {
                    LogInfo($"Request #{requestId}: Request headers:{Environment.NewLine}{Environment.NewLine}{requestHeaders}");
                    LogInfo($"Request #{requestId}: Request body:{Environment.NewLine}{Environment.NewLine}{Encoding.UTF8.GetString(request.uploadHandler?.data ?? new byte[] {})}");
                }            }
            else if (response is FailedWebResponse { Exception: TimeoutException } timeoutResponse)
            {
                LogError($"Request #{requestId}: Request timed out");
                LogError(timeoutResponse.Exception, $"Request #{requestId}: ");
                if (verbose)
                {
                    LogError($"Request #{requestId}: Request headers:{Environment.NewLine}{Environment.NewLine}{requestHeaders}");
                    LogError($"Request #{requestId}: Request body:{Environment.NewLine}{Environment.NewLine}{Encoding.UTF8.GetString(request.uploadHandler?.data ?? new byte[] {})}");
                }            }
            else
            {
                if ((int)response.StatusCode is >= 200 and < 300)
                {
                    LogInfo(message);
                    if (verbose)
                    {
                        LogInfo($"Request #{requestId}: Request headers:{Environment.NewLine}{Environment.NewLine}{requestHeaders}");
                        LogInfo($"Request #{requestId}: Request body:{Environment.NewLine}{Environment.NewLine}{Encoding.UTF8.GetString(request.uploadHandler?.data ?? new byte[] {})}");
                        LogInfo($"Request #{requestId}: Response headers:{Environment.NewLine}{Environment.NewLine}{responseHeaders}");
                        LogInfo($"Request #{requestId}: Response body:{Environment.NewLine}{Environment.NewLine}{response.ResponseText}");
                    }
                }
                else
                {
                    LogWarning(message);
                    if (verbose)
                    {
                        LogWarning($"Request #{requestId}: Request headers:{Environment.NewLine}{Environment.NewLine}{requestHeaders}");
                        LogWarning($"Request #{requestId}: Request body:{Environment.NewLine}{Environment.NewLine}{Encoding.UTF8.GetString(request.uploadHandler?.data ?? new byte[] {})}");
                        LogWarning($"Request #{requestId}: Response headers:{Environment.NewLine}{Environment.NewLine}{responseHeaders}");
                        LogWarning($"Request #{requestId}: Response body:{Environment.NewLine}{Environment.NewLine}{response.ResponseText}");
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
        
        private static void LogException(Exception exception, Action<string, bool> logFunction, string prefix = "")
        {
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
