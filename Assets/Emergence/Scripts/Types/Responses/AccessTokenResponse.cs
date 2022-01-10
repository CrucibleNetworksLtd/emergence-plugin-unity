namespace EmergenceSDK
{
    public struct AccessTokenResponse
    {
        public class AccessToken
        {
            public string signedMessage;
            public string message;
            public string address;
        }

        public class Message
        {
            public AccessToken accessToken;
        }

        public int statusCode;
        public Message message;
    }
}