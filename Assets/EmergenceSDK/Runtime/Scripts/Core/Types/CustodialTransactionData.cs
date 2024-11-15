using EmergenceSDK.Runtime.Types.Responses;
using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Types
{
    public class CustodialTransactionData
    {
        [JsonProperty("rawTransactionWithoutSignature")]
        public RawTransaction RawTransactionWithoutSignature { get; set; }

        [JsonProperty("transactionSignature")]
        public string TransactionSignature { get; set; }

        [JsonProperty("fromEoa")]
        public string FromEoa { get; set; }

        [JsonProperty("rpcUrl")]
        public string RpcUrl { get; set; }
    }
}