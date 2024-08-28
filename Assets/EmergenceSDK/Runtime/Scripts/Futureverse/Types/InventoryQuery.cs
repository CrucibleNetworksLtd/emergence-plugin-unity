using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmergenceSDK.Runtime.Futureverse.Types
{
    public class InventoryQuery
    {
        [JsonProperty("query")]
        public string Query { get; } = @"query Asset($addresses: [ChainAddress!]!, $first: Float) {
            assets(addresses: $addresses, first: $first) {
                edges {
                    node {
                        metadata {
                            properties
                            attributes
                            rawAttributes
                        }
                        collection {
                            chainId
                            chainType
                            location
                            name
                        }
                        tokenId
                        collectionId
                    }
                }
            }
        }";

        [JsonProperty("variables")]
        public QueryVariables Variables { get; }

        public InventoryQuery(List<string> combinedAddress)
        {
            Variables = new QueryVariables
            {
                Addresses = combinedAddress,
                First = 1000
            };
        }

        public class QueryVariables
        {
            [JsonProperty("addresses")]
            public List<string> Addresses { get; set; }

            [JsonProperty("first")]
            public int First { get; set; }
        }
    }
}