using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Types
{
    public class WebResponse
    {
        public bool IsSuccess { get; private set; }
        public string Response { get; private set; }
        public long StatusCode { get; private set; } = 0;
        public DownloadHandler DownloadHandler { get; private set; }

        public WebResponse(bool isSuccess, string response, long statusCode = 0, DownloadHandler downloadHandler = null)
        {
            IsSuccess = isSuccess;
            Response = response;
            StatusCode = statusCode;
            DownloadHandler = downloadHandler;
        }
    }
}