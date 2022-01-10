namespace EmergenceSDK
{
    public class ReinitializeWalletConnectResponse
    {
        public class Message
        {
            public bool disconnected;
        }

        public int statusCode;
        public Message message;
    }
}
