using Newtonsoft.Json;

namespace EmergenceSDK.Types
{
    public class Expiration
    {
        [JsonProperty("expires-on")]
        public long expiresOn;
    }
}