using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Types
{
    public class EncodedCustodialTransactionData
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public Dictionary<string, string> Message { get; set; }
    }
}