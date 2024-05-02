using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class TokenRequestFailedException : LoginStepRequestFailedException<string>
    {
        internal TokenRequestFailedException(Exception exception) : base(null, exception) { }
        internal TokenRequestFailedException(ServiceResponse<string> response) : base(response) { }
    }
}