using System;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public class FuturepassRequestFailedException : Exception
    {
        public readonly ServiceResponse<LinkedFuturepassResponse> Response;

        public FuturepassRequestFailedException(ServiceResponse<LinkedFuturepassResponse> response)
        {
            Response = response;
        }

        public FuturepassRequestFailedException(Exception exception) : base(null, exception) { }
    }
}