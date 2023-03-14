using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public partial class Services
    {
        #region AWS API

        /// <inheritdoc cref="IPersonaService.GetPersonas"/>
        public void GetPersonas(SuccessPersonas success, ErrorCallback errorCallback) => PersonaService.GetPersonas(success, errorCallback);
        
        public void GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback) => PersonaService.GetCurrentPersona(success, errorCallback);
        
        public void CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback) => PersonaService.CreatePersona(persona, success, errorCallback);
        
        public void EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback) => PersonaService.EditPersona(persona, success, errorCallback);
        
        public void DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback) => PersonaService.DeletePersona(persona, success, errorCallback);
        
        public void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback) => PersonaService.SetCurrentPersona(persona, success, errorCallback);

        #region GetAvatars

        public delegate void SuccessAvatars(List<Avatar> avatar);
        public async void AvatarByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback)
        {
            Debug.Log("Getting avatars for address: " + address);
            
            Debug.Log("Get Avatars request started");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;
            
            Debug.Log("Requesting avatars from URL: " + url);
            string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            
            Debug.Log("Avatar response: " + response.ToString());
            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.ToString());
            
            success?.Invoke(avatarResponse.message);
        }
        
        public delegate void SuccessAvatar(Avatar avatar);
        public async void AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback)
        {
            Debug.Log("Getting avatar with id: " + id);
            
            Debug.Log("Get Avatars by id request started");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            Debug.Log("Requesting avatar by id from URL: " + url);
            
            string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            
            Debug.Log("Avatar by id response: " + response);
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.ToString());

            success?.Invoke(avatarResponse.message);
        }

        #endregion GetAvatars
        
        #region InventoryByOwner

        public delegate void SuccessInventoryByOwner(List<InventoryItem> inventoryItems);
        public async void InventoryByOwner(string address, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {

            Debug.Log("Getting inventory for address: " + address);
                Debug.Log("Inventory By Owner request started");
                // string url = LocalEmergenceServer.Instance.Environment().InventoryURL + "byOwner?address=" + address;
                string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "byOwner?address=" + address;
                Debug.Log("Requesting inventory from URL: " + url);
                string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);

                Debug.Log("Inventory response: " + response.ToString());
                InventoryByOwnerResponse inventoryResponse =
                    SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());

                success?.Invoke(inventoryResponse.message.items);
                // StartCoroutine(CoroutineInventoryByOwner(address, success, error));
            
            
        }

        #endregion InventoryByOwner
        
        #region WriteDynamicMetadata
        public delegate void SuccessWriteDynamicMetadata(string response);
        public async void WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            metadata = "{\"metadata\": \"" + metadata + "\"}";
            
            Debug.Log("Writing dynamic metadata for contract: " + contract);
            Debug.Log("Write dynamic metadata request started");
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;
            
            Debug.Log("Dynamic metadata url: " + url);
            Debug.Log("updated metadata: " + metadata);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", "0iKoO1V2ZG98fPETreioOyEireDTYwby");
            string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, errorCallback, metadata, headers);
            
            Debug.Log("Write dynamic metadata response: " + response.ToString());
            BaseResponse<string> dynamicMetadataResponse = SerializationHelper.Deserialize<BaseResponse<string>>(response.ToString());
            
            success?.Invoke(response.ToString());
           
        }
        #endregion WriteDynamicMetadata

        public void OpenNFTPicker(Action<InventoryItem> customOnClickHandler = null)
        {
            EmergenceSingleton.Instance.OpenEmergenceUI();
            ScreenManager.Instance.ShowCollection(customOnClickHandler);
        }

        #endregion AWS API
    }
}