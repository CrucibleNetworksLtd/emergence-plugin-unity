using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Types
{
    public class Expiration
    {
        [JsonProperty("expires-on")]
        public long expiresOn;
    }
}