using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Types
{
    public class CustodialTransaction
    {
        [JsonProperty("eoa")]
        public string Eoa { get; set; }

        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("rpcUrl")]
        public string RpcUrl { get; set; }
    }
}