using System;

namespace EmergenceSDK.Runtime.Types.Exceptions.Login
{
    public sealed class TokenRequestFailedException : LoginStepRequestFailedException<string>
    {
        internal TokenRequestFailedException(string message, ServiceResponse<string> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}