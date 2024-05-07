using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class TokenRequestFailedException : LoginStepRequestFailedException<string>
    {
        internal TokenRequestFailedException(string message, ServiceResponse<string> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}