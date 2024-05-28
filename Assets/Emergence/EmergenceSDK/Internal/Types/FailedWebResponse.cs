using System;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class FailedWebResponse : WebResponse
    {
        public override bool InProgress => false;
        public override bool Successful => false;
        public bool TimedOut => Exception is TimeoutException;
        public bool Canceled => Exception is OperationCanceledException;
        public readonly Exception Exception;

        public FailedWebResponse(Exception exception, UnityWebRequest request) : base(request)
        {
            Exception = exception;
        }
    }
}