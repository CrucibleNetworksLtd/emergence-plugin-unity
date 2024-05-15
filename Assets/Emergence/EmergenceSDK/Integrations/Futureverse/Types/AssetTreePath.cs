using System.Collections.Generic;
using System.Runtime.Serialization;
using EmergenceSDK.Internal.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Integrations.Futureverse.Types
{
    public class AssetTreePath
    {
        [JsonProperty("@id")]
        public string ID;
        [JsonIgnore]
        public string RdfType;
        [JsonIgnore]
        public readonly Dictionary<string, Object> Objects = new();
        [JsonExtensionData]
        private readonly Dictionary<string, JToken> _objects = new();
            
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach (var kvp in _objects)
            {
                Objects.Add(kvp.Key, SerializationHelper.Deserialize<Object>(kvp.Value));
            }
            RdfType = Objects.GetValueOrDefault("rdf:type")?.ID;
        }
        
        public class Object
        {
            [JsonProperty("@id")]
            public string ID;
            [JsonExtensionData]
            public Dictionary<string, JToken> AdditionalData = new();
        }
    }
}