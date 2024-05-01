using System;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public class FuturepassInformationRequestFailedException : Exception
    {
        public readonly ServiceResponse<FuturepassInformationResponse> Response;
        public FuturepassInformationRequestFailedException(ServiceResponse<FuturepassInformationResponse> response)
        {
            Response = response;
        }

        public FuturepassInformationRequestFailedException(Exception exception) : base(null, exception) { }
    }
}