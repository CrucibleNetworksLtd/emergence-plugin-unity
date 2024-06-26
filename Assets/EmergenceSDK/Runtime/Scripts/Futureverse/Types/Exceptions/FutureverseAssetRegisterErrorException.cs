using System;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Runtime.Futureverse.Types.Exceptions
{
    public class FutureverseAssetRegisterErrorException : Exception
    {
        public readonly string Response;
        public readonly JArray Errors;

        public FutureverseAssetRegisterErrorException(string response, JArray errors)
        {
            Response = response;
            Errors = errors;
        }
    }
}