using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class HandshakeRequestFailedException : LoginStepRequestFailedException<string>
    {
        internal HandshakeRequestFailedException(string message, ServiceResponse<string> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}