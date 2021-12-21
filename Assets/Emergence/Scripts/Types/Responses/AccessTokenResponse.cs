namespace Emergence
{
    public struct AccessTokenResponse
    {
        public struct AccessToken
        {
            public string signedMessage;
            public string message;
            public string address;
        }

        public struct Message
        {
            public AccessToken accessToken;
        }

        public int statusCode;
        public Message message;
    }
}