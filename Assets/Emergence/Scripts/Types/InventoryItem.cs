using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK
{
    public class InventoryItem
    {

        public class InventoryItemMeta
        {
            
            
            public class InventoryItemMetaAttributes
            {
                
            }

            public class InventoryItemMetaContent
            {
                // public string type;
                public string url;
                public string representation;
                public string mimeType;
                public int size;
                public int width;
                public int height;
            }

            public class InventoryItemMetaRestrictions
            {
                
            }
            
            public string name;
            public string description;
            public List<InventoryItemMetaAttributes> attributes;
            public List<InventoryItemMetaContent> content;
            public List<InventoryItemMetaRestrictions> restrictions;
            public string dynamicMetadata;
        }
        
        public class InventoryItemCreators
        {
            public string account;
            public int value;
        }
        
        public class InventoryItemPending
        {
                
        }
        
        public string id;
        public string blockchain;
        public string contract;
        public string tokenId;
        public List<InventoryItemCreators> creators;
        // public string owners;
        // public string royalties;
        public string lazySupply;
        public List<InventoryItemPending> pending;
        public string mintedAt;
        public string lastUpdatedAt;
        public string supply;
        public InventoryItemMeta meta;
        public bool deleted;
        public string totalStock;

    }
}