using System;

namespace EmergenceSDK.Runtime.Types.Exceptions.Login
{
    public sealed class HandshakeRequestFailedException : LoginStepRequestFailedException<string>
    {
        internal HandshakeRequestFailedException(string message, ServiceResponse<string> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}