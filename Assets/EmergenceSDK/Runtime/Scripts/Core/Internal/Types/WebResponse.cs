using System;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Internal.Services;
using UnityEngine.Networking;

namespace EmergenceSDK.Runtime.Internal.Types
{
    public class WebResponse : IDisposable
    {
        public UnityWebRequest.Result Result => Request.result;
        public string Error => Request.error;
        public string Url => Request.url;
        public virtual bool InProgress => Result == UnityWebRequest.Result.InProgress;
        public virtual bool Successful => Result == UnityWebRequest.Result.Success;
        public string ResponseText => Request.downloadHandler?.text ?? "";
        public byte[] ResponseBytes => Request.downloadHandler?.data ?? new byte[] {};
        public long StatusCode => Request.responseCode;
        public Dictionary<string, string> Headers { get; }
        protected readonly UnityWebRequest Request;

        public WebResponse(UnityWebRequest request)
        {
            Request = request;
            Headers = request.GetResponseHeaders() ?? new Dictionary<string, string>();
        }

        ~WebResponse()
        {
            Dispose();
        }

        public void Dispose()
        {
            WebRequestService.Instance.RemoveRequest(Request);
            Request?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}