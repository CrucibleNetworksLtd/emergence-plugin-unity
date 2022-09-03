namespace EmergenceSDK
{
    public class AccessTokenResponse
    {
        public class AccessToken
        {
            public string signedMessage;
            public string message;
            public string address;
        }

        public AccessToken accessToken;
    }
}