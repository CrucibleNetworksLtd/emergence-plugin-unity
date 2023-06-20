using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Types.Inventory;
using UnityEngine;

namespace EmergenceSDK.Internal.UI.Inventory
{
    public class InventoryItemStore
    {
        private readonly Func<GameObject> instantiateItemEntry;

        private HashSet<InventoryItem> items = new HashSet<InventoryItem>();
        private Dictionary<string, InventoryItemEntry> entryDictionary = new Dictionary<string, InventoryItemEntry>();
        
        private List<InventoryItemEntry> spentEntries = new List<InventoryItemEntry>();
        
        public InventoryItemStore(Func<GameObject> instantiateItemEntry)
        {
            this.instantiateItemEntry = instantiateItemEntry;
        }
        
        public void SetItems(List<InventoryItem> items)
        {
            var displayItems = items.Where(item => item.Meta != null).ToList();
            foreach (InventoryItem item in displayItems)
            {
                if(AddItem(item))
                    UpdateItem(item);
            }
        }

        private void UpdateItem(InventoryItem item)
        {
            items.Remove(item);
            items.Add(item);
        }

        private bool AddItem(InventoryItem item)
        {
            if (items.Add(item))
            {
                if (!entryDictionary.ContainsKey(item.ID))
                {
                    InventoryItemEntry entry = CreateEntry(item);
                    entryDictionary.Add(item.ID, entry);
                    return true;
                }
            }
            return false;
        }

        private InventoryItemEntry CreateEntry(InventoryItem item)
        {
            GameObject entry;
            if(spentEntries.Count > 0)
            {
                entry = spentEntries[0].gameObject;
                entry.SetActive(true);
                spentEntries.RemoveAt(0);
            }
            else
            {
                entry = instantiateItemEntry();
            }
            InventoryItemEntry entryComponent = entry.GetComponent<InventoryItemEntry>();
            entryComponent.SetItem(item);
            
            return entryComponent;
        }
        
        public InventoryItemEntry GetEntry(string itemId)
        {
            if (entryDictionary.TryGetValue(itemId, out InventoryItemEntry entry))
            {
                return entry;
            }
            return null;
        }

        public List<InventoryItemEntry> GetAllEntries() => new List<InventoryItemEntry>(entryDictionary.Values);

        private void RemoveEntry(string itemId)
        {
            if (entryDictionary.ContainsKey(itemId))
            {
                var entry = entryDictionary[itemId];
                spentEntries.Add(entryDictionary[itemId]);
                entry.gameObject.SetActive(false);

                entryDictionary.Remove(itemId);
            }
            items.RemoveWhere(item => item.ID == itemId);
        }
    }
}
