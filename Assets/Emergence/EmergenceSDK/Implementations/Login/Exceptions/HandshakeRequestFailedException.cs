using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class HandshakeRequestFailedException : LoginStepRequestFailedException<string>
    {
        internal HandshakeRequestFailedException(Exception exception) : base(null, exception) { }
        internal HandshakeRequestFailedException(ServiceResponse<string> response) : base(response) { }
    }
}