using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Types.Responses
{

    public class FVHelperResponse
    {
        [JsonProperty("encodedPayload")]
        public EncodedPayload EncodedPayload { get; set; }

        [JsonProperty("fullSignerUrl")]
        public string FullSignerUrl { get; set; }

        [JsonProperty("rawTransactionWithoutSignature")]
        public RawTransaction RawTransactionWithoutSignature { get; set; }
    }
    
    public class EncodedData
    {
        [JsonProperty("encodedPayload")]
        public EncodedPayload EncodedPayload { get; set; }

        [JsonProperty("fullSignerUrl")]
        public string FullSignerUrl { get; set; }

        [JsonProperty("rawTransactionWithoutSignature")]
        public RawTransaction RawTransactionWithoutSignature { get; set; }
    }

    public class EncodedPayload
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }

    public class Payload
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("idpUrl")]
        public string IdpUrl { get; set; }

        [JsonProperty("callbackUrl")]
        public string CallbackUrl { get; set; }
    }

    public class RawTransaction
    {
        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        [JsonProperty("gasLimit")]
        public string GasLimit { get; set; }

        [JsonProperty("maxFeePerGas")]
        public string MaxFeePerGas { get; set; }

        [JsonProperty("maxPriorityFeePerGas")]
        public string MaxPriorityFeePerGas { get; set; }

        [JsonProperty("nonce")]
        public int Nonce { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
    
    
}