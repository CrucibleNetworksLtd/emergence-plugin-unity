using System;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class FailedWebResponse : WebResponse
    {
        public override bool Completed => false;
        public override bool Successful => false;
        public readonly Exception Exception;
        public FailedWebResponse(Exception exception, UnityWebRequest webRequest) : base(webRequest)
        {
            Exception = exception;
        }
    }
}