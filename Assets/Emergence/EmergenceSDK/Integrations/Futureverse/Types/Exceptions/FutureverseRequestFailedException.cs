using System;
using EmergenceSDK.Internal.Types;

namespace EmergenceSDK.Integrations.Futureverse.Types.Exceptions
{
    public class FutureverseRequestFailedException : Exception
    {
        public readonly WebResponse Response;

        public FutureverseRequestFailedException(WebResponse response)
        {
            Response = response;
        }
    }
}