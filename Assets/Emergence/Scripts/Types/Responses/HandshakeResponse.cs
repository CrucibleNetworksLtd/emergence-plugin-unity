namespace Emergence
{
    public class HandshakeResponse
    {
        public class Message
        {
            public string address;
        }

        public int statusCode;
        public Message message;
    }
}
