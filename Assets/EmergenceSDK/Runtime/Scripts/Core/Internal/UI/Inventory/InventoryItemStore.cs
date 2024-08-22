using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Runtime.Types.Inventory;

namespace EmergenceSDK.Runtime.Internal.UI
{
    public class InventoryItemStore
    {
        private HashSet<InventoryItem> items = new HashSet<InventoryItem>();

        public void SetItems(List<InventoryItem> itemsIn)
        {
            items.Clear();

            if (itemsIn != null && itemsIn.Count > 0)
            {
                foreach (var item in itemsIn)
                {
                    if (item.Meta != null)
                    {
                        items.Add(item);
                    }
                }
            }
        }

        public bool AddItem(InventoryItem item)
        {
            return items.Add(item);
        }

        public void UpdateItem(InventoryItem item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                items.Add(item);
            }
        }

        public InventoryItem GetItem(string itemId)
        {
            return items.FirstOrDefault(item => item.ID == itemId);
        }

        public List<InventoryItem> GetAllItems() => new List<InventoryItem>(items);

        public void RemoveItem(string itemId)
        {
            items.RemoveWhere(item => item.ID == itemId);
        }
    }
}