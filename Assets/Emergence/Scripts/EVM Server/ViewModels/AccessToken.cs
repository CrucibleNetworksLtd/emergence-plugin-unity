using Newtonsoft.Json;

namespace EmergenceEVMLocalServer.ViewModels
{
    public class AccessToken
    {
        public string signedMessage { get; set; }
        public string message { get; set; }
        public string address { get; set; }
    }

    public class MessageContent
    {
        [JsonProperty("expires-on")]
        public int expiresOn { get; set; }
    }
}
