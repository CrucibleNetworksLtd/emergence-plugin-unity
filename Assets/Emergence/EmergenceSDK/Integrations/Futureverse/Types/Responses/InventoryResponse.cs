using System.Collections.Generic;
using EmergenceSDK.Internal.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Integrations.Futureverse.Types.Responses
{
    [SerializationHelper.StoreOriginalJTokens]
    public class InventoryResponse
    {
        public class Data
        {
            public class Assets
            {
                public class Edge
                {
                    public class Node
                    {
                        public class Collection
                        {
                            public int chainId { get; set; }
                            public string chainType { get; set; }
                            public string location { get; set; }
                            public string name { get; set; }
                        }

                        public class Metadata
                        {
                            public class Properties
                            {
                                public string image { get; set; }
                                public string base_image { get; set; }
                                public Dictionary<string, string> models { get; set; }
                                public string animation_url { get; set; }
                                public string external_url { get; set; }
                                public int tokenId { get; set; }
                                public string transparent_image { get; set; }
                                public string name { get; set; }
                                public string last_name { get; set; }
                                public string first_name { get; set; }
                                public string description { get; set; }
                                public string _id { get; set; }
                            }
                            
                            public Properties properties { get; set; }
                            public Dictionary<string, string> attributes { get; set; }
                        }

                        public string assetType { get; set; }
                        public Collection collection { get; set; }
                        public int tokenId { get; set; }
                        public string collectionId { get; set; }
                        public Metadata metadata { get; set; }

                        [field: SerializationHelper.OriginalJToken]
                        public JToken OriginalData { get; } = null;
                    }
                    
                    public Node node { get; set; }
                }
                
                public List<Edge> edges { get; set; }
            }
            
            public Assets assets { get; set; }
        }
        
        public Data data { get; set; }
    }
}
