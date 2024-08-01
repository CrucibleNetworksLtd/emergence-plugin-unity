using System;
using EmergenceSDK.Runtime.Futureverse.Types.Responses;

namespace EmergenceSDK.Runtime.Types.Exceptions.Login
{
    public sealed class FuturepassInformationRequestFailedException : LoginStepRequestFailedException<FuturepassInformationResponse>
    {
        internal FuturepassInformationRequestFailedException(string message, ServiceResponse<FuturepassInformationResponse> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}