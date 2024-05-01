using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public class TokenRequestFailedException : Exception
    {
        public readonly ServiceResponse<string> Response;

        public TokenRequestFailedException(ServiceResponse<string> response)
        {
            Response = response;
        }

        public TokenRequestFailedException(Exception exception) : base(null, exception) { }
    }
}