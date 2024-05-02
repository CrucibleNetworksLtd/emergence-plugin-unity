using System;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public sealed class FuturepassInformationRequestFailedException : LoginStepRequestFailedException<FuturepassInformationResponse>
    {
        internal FuturepassInformationRequestFailedException(Exception exception) : base(null, exception) { }
        internal FuturepassInformationRequestFailedException(ServiceResponse<FuturepassInformationResponse> response) : base(response) { }
    }
}