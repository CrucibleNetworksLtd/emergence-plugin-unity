using System.Collections.Generic;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class InventoryService : DemoStation<InventoryService>, IDemoStation
    {
        [SerializeField] private GameObject itemEntryPrefab;
        [SerializeField] private GameObject contentGO;
        [SerializeField] private GameObject scrollView;

        private List<GameObject> items = new List<GameObject>();

        private bool isInventoryVisible = false;
        
        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
                isReady = value;
            }
        }

        private void Start()
        {
            instructionsGO.SetActive(false);
            IsReady = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated() && IsReady)
            {
                ShowInventory();
            }
        }

        public void ShowInventory()
        {
            if (!isInventoryVisible)
            {
                DG.Tweening.DOTween.To(() => scrollView.GetComponent<RectTransform>().anchoredPosition,
                    x => scrollView.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, 0), 0.25f);
                isInventoryVisible = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                DG.Tweening.DOTween.To(() => scrollView.GetComponent<RectTransform>().anchoredPosition,
                    x => scrollView.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, -200f), 0.25f);
                isInventoryVisible = false;
                Cursor.visible = false;
            }

            EmergenceServices.Instance.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), SuccessInventoryByOwner, ErrorCallback);
        }

        private void ErrorCallback(string error, long code)
        {
            Debug.LogError("[" + code + "] " + error);
        }

        private void SuccessInventoryByOwner(List<InventoryItem> inventoryItems)
        {
            foreach (var item in items)
            {
                Destroy(item);
            }

            items.Clear();

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                GameObject entry = Instantiate(itemEntryPrefab, contentGO.transform, false);

                InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                itemEntry.SetItem(inventoryItems[i]);

                items.Add(entry);
            }
        }
    }

}