namespace EmergenceSDK.Types
{
    public class WebResponse
    {
        public bool IsSuccess { get; private set;}
        public string Response { get; private set; }
        
        public WebResponse(bool isSuccess, string response)
        {
            IsSuccess = isSuccess;
            Response = response;
        }
    }
}