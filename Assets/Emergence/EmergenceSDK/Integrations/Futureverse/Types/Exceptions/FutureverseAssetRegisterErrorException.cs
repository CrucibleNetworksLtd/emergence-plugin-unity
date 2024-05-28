using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Integrations.Futureverse.Types.Exceptions
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