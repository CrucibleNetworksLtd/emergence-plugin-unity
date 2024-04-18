namespace EmergenceSDK.Types
{
    public class ValidateSignedMessageRequest
    {
        public string message { get; set; }
        public string signedMessage { get; set; }
        public string address { get; set; }

        public ValidateSignedMessageRequest(string message, string signedMessage, string address)
        {
            this.message = message;
            this.signedMessage = signedMessage;
            this.address = address;
        }
    }
}
