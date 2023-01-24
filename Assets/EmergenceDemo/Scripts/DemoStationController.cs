using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using EmergenceSDK;
using DG.Tweening;

public class DemoStationController : MonoBehaviour
{
    [SerializeField] private GameObject instructions;
    
    [SerializeField] private string contract;
    [SerializeField] private string ABI;

    [SerializeField] private GameObject itemEntryPrefab;
    [SerializeField] private GameObject contentGO;
    [SerializeField] private GameObject scrollView;

    private bool isInventoryVisible = false;

    public UnityEvent invokeMethod;

    void Start()
    {
        instructions.SetActive(false);
        scrollView.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200f);
    }

    private void OnTriggerEnter(Collider other)
    {
        instructions.SetActive(true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        instructions.SetActive(false);
    }

    public void OpenOverlay()
    {
        EmergenceSingleton.Instance.OpenEmergenceUI();
    }
    
    public void GetCurrentCount()
    {
        ContractHelper.LoadContract(contract, ABI, () =>
        {
            ContractHelper.ReadMethod<ReadContractTokenURIResponse, string[]>(contract, "GetCurrentCount", new string[] { "" },
                (response) =>
                {
                    Debug.Log("Current count: " + response.response);
                }, (message, id) =>
                {
                    Debug.LogError("Error while getting current count: " + message);
                });
        }, (message, id) =>
        {
            Debug.LogError("Error while loading contract: " + message);
        });
    }
    
    public void IncrementCount()
    {
        ContractHelper.LoadContract(contract, ABI, () =>
        {
            ContractHelper.WriteMethod<ReadContractTokenURIResponse, string[]>(contract, "IncrementCount", "", "", new string[] {  },
                (response) =>
                {
                    Debug.Log("Increment count: " + response.response);
                }, (message, id) =>
                {
                    Debug.LogError("Error while incrementing count: " + message);
                });
        }, (message, id) =>
        {
            Debug.LogError("Error while loading contract: " + message);
        });
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
                // Debug.Log("Received items: " + inventoryItems.Count);
                // Modal.Instance.Show("Retrieving inventory items...");
                    
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    GameObject entry = Instantiate(itemEntryPrefab);

                    Button entryButton = entry.GetComponent<Button>();
                    // entryButton.onClick.AddListener(OnInventoryItemPressed);
                        
                    InventoryItemEntry itemEntry = entry.GetComponent<InventoryItemEntry>();
                    itemEntry.itemName.text = inventoryItems[i]?.meta?.name;
                    itemEntry.url = inventoryItems[i]?.meta?.content?.First().url;
                    itemEntry.SetImageUrl(inventoryItems[i]?.meta?.content?.First().url);
                        
                    entry.transform.SetParent(contentGO.transform, false);
                }
                    
                // Modal.Instance.Hide();
                    
            },
            (error, code) =>
            {
                Debug.LogError("[" + code + "] " + error);
                // Modal.Instance.Hide();
            });
    }
}
