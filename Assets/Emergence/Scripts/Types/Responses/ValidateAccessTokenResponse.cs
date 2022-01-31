namespace EmergenceSDK
{
    public class ValidateAccessTokenResponse
    {
        public class Message
        {
            public bool valid;
        }

        public int statusCode;
        public Message message;
    }
}