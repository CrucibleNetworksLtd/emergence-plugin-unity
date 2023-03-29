using System.Collections.Generic;

namespace EmergenceSDK.Types.Inventory
{
    public class InventoryItemMetaData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<InventoryItemMetaAttributes> Attributes { get; set; }
        public List<InventoryItemMetaContent> Content { get; set; }
        public List<object> Restrictions { get; set; }
        public string DynamicMetadata { get; set; }
    }
}