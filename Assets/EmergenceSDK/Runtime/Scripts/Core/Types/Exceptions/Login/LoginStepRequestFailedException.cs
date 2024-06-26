using System;

namespace EmergenceSDK.Runtime.Types.Exceptions.Login
{
    public abstract class LoginStepRequestFailedException<T> : Exception
    {
        /// <summary>
        /// Returned response, or null if the request failed before a response could be returned.
        /// </summary>
        public readonly ServiceResponse<T> Response;

        internal LoginStepRequestFailedException(string message, ServiceResponse<T> response = null, Exception exception = null) : base(message, exception) { Response = response; }
    }
}