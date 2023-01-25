using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EmergenceSDK;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DemoInventoryService : MonoBehaviour
{
    [SerializeField] private GameObject itemEntryPrefab;
    [SerializeField] private GameObject contentGO;
    [SerializeField] private GameObject scrollView;
    public GameObject instructions;
    
    private bool isInventoryVisible = false;
    
    private void Start()
    {
        instructions.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        instructions.SetActive(true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        instructions.SetActive(false);
    }
    
    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && instructions.activeSelf)
        {
            ShowInventory();
        }
    }
    
    public void ShowInventory()
    {
        if (!isInventoryVisible)
        {
            DOTween.To(() => scrollView.GetComponent<RectTransform>().anchoredPosition,
                x=> scrollView.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, 0), 0.25f);
            isInventoryVisible = true;
        }
        else
        {
            DOTween.To(() => scrollView.GetComponent<RectTransform>().anchoredPosition,
                x=> scrollView.GetComponent<RectTransform>().anchoredPosition = x, new Vector2(0, -200f), 0.25f);
            isInventoryVisible = false;
        }
        
        Services.Instance.InventoryByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (inventoryItems) =>
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    GameObject entry = Instantiate(itemEntryPrefab);

                    Button entryButton = entry.GetComponent<Button>();
                    // entryButton.onClick.AddListener(OnInventoryItemPressed);
                        
                    InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                    itemEntry.SetItem(inventoryItems[i]);
                    // itemEntry.itemName.text = inventoryItems[i]?.meta?.name;
                    // itemEntry.url = inventoryItems[i]?.meta?.content?.First().url;
                    // itemEntry.SetImageUrl(inventoryItems[i]?.meta?.content?.First().url);
                        
                    entry.transform.SetParent(contentGO.transform, false);
                }
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
            });
    }
}

