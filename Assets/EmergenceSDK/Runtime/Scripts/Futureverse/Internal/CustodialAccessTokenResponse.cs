using System;
using System.Text;
using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Futureverse.Internal
{
    public class CustodialAccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }
        
        [JsonProperty("id_token")]
        public string RawIdToken
        {
            get => rawToken;
            set
            {
                rawToken = value;
                DecodedToken = DecodeIdToken(value);
            } 
        }

        private string rawToken;

        [JsonIgnore] 
        public CustodialIdToken DecodedToken;

        [JsonProperty("expires_in")]
        private int ExpiresInSeconds { get; set; }

        [JsonIgnore]
        public DateTime Expiration => DateTime.UtcNow.AddSeconds(ExpiresInSeconds);

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        private CustodialIdToken DecodeIdToken(string rawToken)
        {
            // Split the JWT into its components
            var parts = rawToken.Split('.');
            if (parts.Length < 2)
            {
                throw new FormatException("Invalid ID token format.");
            }

            // Decode the payload (second part of the JWT)
            var payload = parts[1];
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var idTokenBytes = Convert.FromBase64String(payload);
            var custodialToken = JsonConvert.DeserializeObject<CustodialIdToken>(Encoding.UTF8.GetString(idTokenBytes));
            return custodialToken;
        }
    }
    
    /// <summary>
    /// Represents the payload of the ID token from the Futureverse authentication.
    /// </summary>
    [Serializable]
    public class CustodialIdToken
    {
        /// <summary>
        /// The subject identifier.
        /// </summary>
        [JsonProperty("sub")]
        public string Sub { get; set; }

        /// <summary>
        /// The Ethereum address.
        /// </summary>
        [JsonProperty("eoa")]
        public string Eoa { get; set; }

        /// <summary>
        /// The custodian information.
        /// </summary>
        [JsonProperty("custodian")]
        public string Custodian { get; set; }

        /// <summary>
        /// The chain ID.
        /// </summary>
        [JsonProperty("chainId")]
        public int ChainId { get; set; }

        /// <summary>
        /// The user email.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The futurepass string.
        /// </summary>
        [JsonProperty("futurepass")]
        public string Futurepass { get; set; }

        /// <summary>
        /// The authentication time (Unix timestamp).
        /// </summary>
        [JsonProperty("auth_time")]
        public long AuthTime { get; set; }

        /// <summary>
        /// The nonce for replay protection.
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// The access token hash.
        /// </summary>
        [JsonProperty("at_hash")]
        public string AtHash { get; set; }

        /// <summary>
        /// The audience (client ID).
        /// </summary>
        [JsonProperty("aud")]
        public string Aud { get; set; }

        /// <summary>
        /// The expiration time (Unix timestamp).
        /// </summary>
        [JsonProperty("exp")]
        public long Exp { get; set; }

        /// <summary>
        /// The issued at time (Unix timestamp).
        /// </summary>
        [JsonProperty("iat")]
        public long Iat { get; set; }

        /// <summary>
        /// The issuer URL.
        /// </summary>
        [JsonProperty("iss")]
        public string Iss { get; set; }
    }
}