using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Types.Responses
{
    public class CustodialSignerServiceResponse
    {
        [JsonProperty("result")]
        public Result Result { get; set; }

        [JsonProperty("payload")]
        public SignerServicePayload Payload { get; set; }
    }

    public class Result
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }

    public class SignerServicePayload
    {
        [JsonProperty("account")]
        public string Account { get; set; }
    }
}