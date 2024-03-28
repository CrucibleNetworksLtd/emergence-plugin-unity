using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmergenceSDK.Integrations.Futureverse.Types
{
    public class InventoryQuery
    {
        [JsonProperty("query")]
        public string Query { get; } = @"query ExampleQuery($addresses: [Address]!, $chainLocations: [BlockchainLocationInput], $first: Int) {
        nfts(addresses: $addresses, chainLocations: $chainLocations, first: $first) {
            edges {
                node {
                    assetType
                    collection {
                        chainId
                        chainType
                        location
                        name
                    }
                    tokenIdNumber
                    metadata {
                        properties
                    }
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
                First = 100
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