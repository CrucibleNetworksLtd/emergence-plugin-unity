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
            Creators = new List<InventoryItemCreators>(other.Creators);
            Owners = other.Owners;
            Royalties = other.Royalties;
            LazySupply = other.LazySupply;
            Pending = new List<object>(other.Pending);
            MintedAt = other.MintedAt;
            LastUpdatedAt = other.LastUpdatedAt;
            Supply = other.Supply;
            Meta = new InventoryItemMetaData(other.Meta);
            Deleted = other.Deleted;
            TotalStock = other.TotalStock;
        }
    }
}