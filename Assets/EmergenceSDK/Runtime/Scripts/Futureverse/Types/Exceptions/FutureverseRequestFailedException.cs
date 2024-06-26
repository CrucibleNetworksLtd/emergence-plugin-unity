using System;
using EmergenceSDK.Runtime.Internal.Types;

namespace EmergenceSDK.Runtime.Futureverse.Types.Exceptions
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