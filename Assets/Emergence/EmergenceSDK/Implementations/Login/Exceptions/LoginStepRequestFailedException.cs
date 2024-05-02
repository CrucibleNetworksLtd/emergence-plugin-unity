using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public abstract class LoginStepRequestFailedException<T> : Exception
    {
        /// <summary>
        /// Returned response, or null if the request failed before a response could be returned.
        /// </summary>
        public readonly ServiceResponse<T> Response;
        
        internal LoginStepRequestFailedException(ServiceResponse<T> response) { Response = response; }
        internal LoginStepRequestFailedException(string message, Exception exception) : base(message, exception) { }
    }
}