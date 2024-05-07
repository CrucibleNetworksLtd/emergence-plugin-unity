using System;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class TimeoutWebResponse : WebResponse
    {
        public readonly TimeoutException Exception;
        public TimeoutWebResponse(TimeoutException exception) : base(false, "Request timed out.")
        {
            Exception = exception;
        }
    }
}