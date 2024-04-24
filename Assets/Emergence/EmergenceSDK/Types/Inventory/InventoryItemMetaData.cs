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

        public InventoryItemMetaData()
        {
            // Default constructor
        }

        public InventoryItemMetaData(InventoryItemMetaData other)
        {
            Name = other.Name;
            Description = other.Description;
            Attributes = new List<InventoryItemMetaAttributes>(other.Attributes ?? new List<InventoryItemMetaAttributes>());
            Content = new List<InventoryItemMetaContent>(other.Content ?? new List<InventoryItemMetaContent>());
            Restrictions = new List<object>(other.Restrictions ?? new List<object>());
            DynamicMetadata = other.DynamicMetadata;
        }
    }
}