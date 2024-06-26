using System;

namespace EmergenceSDK.Runtime.Types.Exceptions
{
    class SignMessageFailedException : Exception
    {
        public readonly string Response;
        public SignMessageFailedException(string response)
        {
            Response = response;
        }
    }
}