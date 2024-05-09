using System;
using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.Internal.Services;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class WebResponse : IDisposable
    {
        public UnityWebRequest.Result Result => Request.result;
        public string Error => Request.error;
        public virtual bool Completed => Result != UnityWebRequest.Result.InProgress;
        public virtual bool Successful => Result == UnityWebRequest.Result.Success;
        public string ResponseText => Request.downloadHandler?.text ?? "";
        public byte[] ResponseBytes => Request.downloadHandler?.data ?? new byte[] {};
        public long StatusCode => Request.responseCode;
        public Dictionary<string, string> Headers { get; }
        /// <summary>
        /// We shouldn't have any permanent reference to this, as this will be disposed when the WebRequest gets Finalized or manually Disposed
        /// </summary>
        public readonly UnityWebRequest Request;

        public WebResponse(UnityWebRequest request)
        {
            Request = request;
            Headers = request.GetResponseHeaders() ?? new ();
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