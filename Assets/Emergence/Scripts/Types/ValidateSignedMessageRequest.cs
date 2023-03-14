namespace EmergenceSDK.Types
{
    public class ValidateSignedMessageRequest
    {
        public string message { get; set; }
        public string signedMessage { get; set; }
        public string address { get; set; }
    }
}
