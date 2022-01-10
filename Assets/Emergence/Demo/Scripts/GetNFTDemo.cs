using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class GetNFTDemo : MonoBehaviour
    {
        [Header("Contract information")]
        public string contractAddress = "0x9498274B8C82B4a3127D67839F2127F2Ae9753f4";
        [TextArea(15, 20)]
        public string ABI = "[{'inputs':[{'internalType':'string','name':'name','type':'string'},{'internalType':'string','name':'symbol','type':'string'}],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'approved','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'operator','type':'address'},{'indexed':false,'internalType':'bool','name':'approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'TokenMinted','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'approve','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'player','type':'address'},{'internalType':'string','name':'tokenURI','type':'string'}],'name':'mint','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'bytes','name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'operator','type':'address'},{'internalType':'bool','name':'approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'bytes4','name':'interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'tokenURI','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'}]";
        public string[] data = new string[] { "1" };

        [Header("Scene references")]
        public MeshRenderer NFTmeshRenderer;
        public Button downloadButton;

        private Material materialInstance;

        private void Awake()
        {
            downloadButton.onClick.AddListener(OnDownloadClicked);
        }

        private void Start()
        {
            // This assignment creates a fresh instance of the material,
            // so we don't touch the asset file of the material
            materialInstance = NFTmeshRenderer.material;

            Loader.Instance.OnEmergenceUIVisibilityChanged.AddListener((visible) => { Debug.Log($"UI visible {visible}"); });
            Loader.Instance.OnEmergenceUIOpened.AddListener(() => { Debug.Log("UI OPEN"); });
            Loader.Instance.OnEmergenceUIClosed.AddListener(() => { Debug.Log("UI CLOSED"); });
        }

        private void OnDownloadClicked()
        {
                Debug.Log("Requesting NFT");
                downloadButton.interactable = false;
                downloadButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Downloading...";

                ContractHelper.LoadContract(contractAddress, ABI, () =>
                {
                    Debug.Log("Contract loaded");
                    ContractHelper.ReadContract<ReadContractTokenURIResponse, string[]>(contractAddress, "tokenURI", data, (response) =>
                    {
                        Debug.Log("Contract Read");
                        StartCoroutine(GetMetadata(response.message.response));
                    },
                    (error, code) =>
                    {
                        RestoreButton();
                        Debug.LogError("[" + code + "] " + error);
                    });
                },
                (error, code) =>
                {
                    RestoreButton();
                    Debug.LogError("[" + code + "] " + error);
                });
        }

        private void RestoreButton()
        {
            downloadButton.interactable = true;
            downloadButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Download NFT";
        }

        private IEnumerator GetMetadata(string metadataURL)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(metadataURL))
            {
                yield return request.SendWebRequest();

                if (request.isDone)
                {
                    Debug.Log("Metadata Read");
                    NFTMetadataResponse response = SerializationHelper.Deserialize<NFTMetadataResponse>(request.downloadHandler.text);

                    RequestImage.Instance.AskForImage(response.image, (url, texture) =>
                    {
                        Debug.Log("NFT image downloaded");
                        RestoreButton();
                        materialInstance.mainTexture = texture;
                    },
                    (url, error, code) =>
                    {
                        RestoreButton();
                        Debug.LogError("[" + url + "] " + error + " " + code);
                    });
                }
                else
                {
                    RestoreButton();
                    Debug.LogError("[" + request.responseCode + "] " + request.error);
                }
            }
        }
    }
}
