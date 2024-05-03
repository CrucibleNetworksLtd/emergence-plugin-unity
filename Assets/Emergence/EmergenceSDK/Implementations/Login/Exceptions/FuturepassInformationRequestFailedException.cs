using System;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class FuturepassInformationRequestFailedException : LoginStepRequestFailedException<FuturepassInformationResponse>
    {
        internal FuturepassInformationRequestFailedException(string message, ServiceResponse<FuturepassInformationResponse> response = null, Exception exception = null) : base(message, response, exception) {}
    }
}