using System;
using EmergenceSDK.Runtime.Types.Responses;

namespace EmergenceSDK.Runtime.Types.Exceptions.Login
{
    public sealed class FuturepassRequestFailedException : LoginStepRequestFailedException<LinkedFuturepassResponse>
    {
        internal FuturepassRequestFailedException(string message, ServiceResponse<LinkedFuturepassResponse> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}