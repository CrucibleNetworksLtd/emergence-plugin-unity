using System;
using System.Collections.Generic;

namespace EmergenceSDK.Types.Inventory
{
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
    }
}