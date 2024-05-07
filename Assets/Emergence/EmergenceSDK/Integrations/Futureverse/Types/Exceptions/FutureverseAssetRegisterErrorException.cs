using System;

namespace EmergenceSDK.Integrations.Futureverse.Types.Exceptions
{
    public class FutureverseAssetRegisterErrorException : Exception
    {
        public readonly string Response;
        public FutureverseAssetRegisterErrorException(string response)
        {
            Response = response;
        }
    }
}