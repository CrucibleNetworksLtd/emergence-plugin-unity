using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    
    /// <summary>
    /// Error logger, used to log HTTP errors to the console.
    /// <remarks>Set <see cref="LogToConsole"/> to false to disable error logging if desired</remarks>
    /// </summary>
    public static class ErrorLogger 
    {
        public static bool LogToConsole = true;

        public static void LogError(string error, long errorCode) 
        {
            if (!LogToConsole)
                return;

            bool isError = true;
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
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] frames = trace.GetFrames();
            if (frames.Length >= 3) // Get the class name from the caller's caller
            {
                callingClass = frames[2].GetMethod().DeclaringType.FullName;
            }

            if (isError)
            {
                Debug.LogError($"{errorCode} Error in {callingClass}: {error}");
            }
            else
            {
                Debug.LogWarning($"{errorCode} Warning in {callingClass}: {error}");
            }
        }

    }
}