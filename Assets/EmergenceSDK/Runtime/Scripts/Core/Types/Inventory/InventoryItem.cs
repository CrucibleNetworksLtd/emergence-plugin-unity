using System;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Internal.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Runtime.Types.Inventory
{
    [SerializationHelper.StoreOriginalJTokens]
    public class InventoryItem
    {
        public string ID { get; set; }
        public string Blockchain { get; set; }
        public string Contract { get; set; }
        public string TokenId { get; set; }
        public List<InventoryItemCreators> Creators { get; set; }
        public object Owners { get; set; }
        public object Royalties { get; set; }
        public string LazySupply { get; set; }
        public List<object> Pending { get; set; }
        public DateTime MintedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string Supply { get; set; }
        public InventoryItemMetaData Meta { get; set; }
        public bool Deleted { get; set; }
        public string TotalStock { get; set; }
        
        [SerializationHelper.OriginalJToken][JsonIgnore]
        public JToken OriginalData { get; internal set; }

        public InventoryItem()
        {
            // Default constructor
        }

        public InventoryItem(InventoryItem other)
        {
            ID = other.ID;
            Blockchain = other.Blockchain;
            Contract = other.Contract;
            TokenId = other.TokenId;
            Creators = other.Creators != null ? new List<InventoryItemCreators>(other.Creators) : new List<InventoryItemCreators>();
            Owners = other.Owners;
            Royalties = other.Royalties;
            LazySupply = other.LazySupply;
            Pending = other.Pending != null ? new List<object>(other.Pending) : new List<object>();
            MintedAt = other.MintedAt;
            LastUpdatedAt = other.LastUpdatedAt;
            Supply = other.Supply;
            Meta = new InventoryItemMetaData(other.Meta);
            Deleted = other.Deleted;
            TotalStock = other.TotalStock;
            OriginalData = other.OriginalData;
        }
    }
}