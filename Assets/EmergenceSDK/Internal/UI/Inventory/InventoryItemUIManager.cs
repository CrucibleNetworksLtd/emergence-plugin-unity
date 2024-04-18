using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Types.Inventory;
using UnityEngine;

namespace EmergenceSDK.Internal.UI.Inventory
{
    public class InventoryItemUIManager
    {
        private const int MaxDisplayedEntries = 15;
        public bool IsNextPageAvailable => (page + 1) * MaxDisplayedEntries < displayableItems.Count;
        public bool IsPreviousPageAvailable => page > 0 && displayableItems.Count > MaxDisplayedEntries;
        
        private readonly Func<GameObject> instantiateItemEntry;
        private InventoryItemStore itemStore;
        private Dictionary<string, InventoryItemEntry> entryDictionary = new Dictionary<string, InventoryItemEntry>();
        private List<InventoryItemEntry> spentEntries = new List<InventoryItemEntry>();
        private List<InventoryItem> displayableItems = new List<InventoryItem>();
        private int page = 0;

        public InventoryItemUIManager(Func<GameObject> instantiateItemEntry, InventoryItemStore itemStore)
        {
            this.instantiateItemEntry = instantiateItemEntry;
            this.itemStore = itemStore;
        }

        public void NextPage()
        {
            if (IsNextPageAvailable)
            {
                page++;
                UpdateDisplayedEntries();
            }
        }

        public void PreviousPage()
        {
            if (IsPreviousPageAvailable)
            {
                page--;
                UpdateDisplayedEntries();
            }
        }
        
        public void UpdateDisplayItems()
        {
            // Update the list of displayable items
            displayableItems = itemStore.GetAllItems().Where(item => item.Meta != null).ToList();

            // Remove items that are no longer in the list
            var removedItems = entryDictionary.Keys.Except(displayableItems.Select(item => item.ID)).ToList();
            foreach (var itemId in removedItems)
            {
                RemoveEntry(itemId);
            }

            UpdateDisplayedEntries();
        }

        private void UpdateDisplayedEntries()
        {

            // Determine which items to display on the current page
            var itemsToDisplay = displayableItems.Skip(page * MaxDisplayedEntries).Take(MaxDisplayedEntries).ToArray();
            
            // Add or update items in the UI
            foreach (var item in itemsToDisplay)
            {
                if (!AddItem(item))
                    UpdateItem(item);
            }
            
            // Hide all entries
            foreach (var item in displayableItems)
            {
                if (entryDictionary.TryGetValue(item.ID, out InventoryItemEntry entry))
                {
                    entry.gameObject.SetActive(false);
                }
            }

            // Show entries for the current page
            foreach (var item in itemsToDisplay)
            {
                entryDictionary[item.ID].gameObject.SetActive(true);
            }
        }

        private void UpdateItem(InventoryItem item)
        {
            if (entryDictionary.TryGetValue(item.ID, out InventoryItemEntry entry))
            {
                entry.SetItem(item);
            }
        }

        private bool AddItem(InventoryItem item)
        {
            if (!entryDictionary.ContainsKey(item.ID))
            {
                InventoryItemEntry entry = CreateEntry(item);
                entryDictionary.Add(item.ID, entry);
                return true;
            }
            return false;
        }

        private InventoryItemEntry CreateEntry(InventoryItem item)
        {
            GameObject entry;
            if (spentEntries.Count > 0)
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

        private void RemoveEntry(string itemId)
        {
            if (entryDictionary.ContainsKey(itemId))
            {
                var entry = entryDictionary[itemId];
                spentEntries.Add(entryDictionary[itemId]);
                entry.gameObject.SetActive(false);

                entryDictionary.Remove(itemId);
            }
            itemStore.RemoveItem(itemId);
        }

        public List<InventoryItemEntry> GetAllEntries() => new List<InventoryItemEntry>(entryDictionary.Values);
    }
}
