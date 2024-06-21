namespace EmergenceSDK.Types.Responses
{
    public class AccessTokenResponse
    {
        public AccessToken AccessToken;
    }
    public class AccessToken
    {
        public string signedMessage;
        public string message;
        public string address;
    }
}