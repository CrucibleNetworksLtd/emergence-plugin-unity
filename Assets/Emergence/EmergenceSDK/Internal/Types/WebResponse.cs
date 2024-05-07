using System;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class WebResponse : IDisposable
    {
        public UnityWebRequest.Result Result => WebRequest.result;
        public string Error => WebRequest.error;
        public virtual bool Completed => Result != UnityWebRequest.Result.InProgress;
        public virtual bool Successful => Result == UnityWebRequest.Result.Success;
        public string ResponseText => WebRequest.downloadHandler.text;
        public byte[] ResponseBytes => WebRequest.downloadHandler.data;
        public long StatusCode => WebRequest.responseCode;
        public readonly UnityWebRequest WebRequest;

        public WebResponse(UnityWebRequest webRequest)
        {
            WebRequest = webRequest;
        }

        public void Dispose()
        {
            WebRequest?.Dispose();
        }
    }
}