namespace Emergence
{
    public class IsConnectedResponse
    {
        public class Message
        {
            public bool isConnected;
            public string address;
        }

        public int statusCode;
        public Message message;
    }
}
