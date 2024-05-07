using System;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class FuturepassRequestFailedException : LoginStepRequestFailedException<LinkedFuturepassResponse>
    {
        internal FuturepassRequestFailedException(string message, ServiceResponse<LinkedFuturepassResponse> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}