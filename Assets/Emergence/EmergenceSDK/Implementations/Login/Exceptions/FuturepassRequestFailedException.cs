using System;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class FuturepassRequestFailedException : LoginStepRequestFailedException<LinkedFuturepassResponse>
    {
        internal FuturepassRequestFailedException(Exception exception) : base(null, exception) { }
        internal FuturepassRequestFailedException(ServiceResponse<LinkedFuturepassResponse> response) : base(response) { }
    }
}