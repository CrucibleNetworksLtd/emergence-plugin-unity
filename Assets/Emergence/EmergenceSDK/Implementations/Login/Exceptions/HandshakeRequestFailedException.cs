using System;
using EmergenceSDK.Types;

namespace EmergenceSDK.Implementations.Login.Exceptions
{
    public class HandshakeRequestFailedException : Exception
    {
        public readonly ServiceResponse<string> Response;

        public HandshakeRequestFailedException(ServiceResponse<string> response)
        {
            Response = response;
        }

        public HandshakeRequestFailedException(Exception exception) : base(null, exception) { }
    }
}