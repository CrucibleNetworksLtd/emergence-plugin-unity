using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using UniVRM10;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK
{
    public partial class Services
    {
        #region AWS API

        #region GetPersonas

        public delegate void SuccessPersonas(List<Persona> personas, Persona currentPersona);
        public void GetPersonas(SuccessPersonas success, GenericError error)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetPersonas(success, error));
        }

        private IEnumerator CoroutineGetPersonas(SuccessPersonas success, GenericError error)
        {
            Debug.Log("GetPersonas request started");
            // string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "personas";
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("GetPersonas", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    PersonasResponse personasResponse = SerializationHelper.Deserialize<PersonasResponse>(request.downloadHandler.text);
                    CurrentPersona = personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected);
                    success?.Invoke(personasResponse.personas, CurrentPersona);
                }
            }
        }

        #endregion GetPersonas

        #region GetCurrentPersona

        public delegate void PersonaUpdated(Persona persona);
        public static event PersonaUpdated OnCurrentPersonaUpdated;

        private Persona cachedPersona;
        public Persona CurrentPersona
        {
            get
            {
                return cachedPersona;
            }

            private set {
                if (cachedPersona != null && cachedPersona.id.Equals(value.id)) return; // don't do anything if the new persona is the same as the cached one)
                cachedPersona = value;
                OnCurrentPersonaUpdated?.Invoke(cachedPersona);
            }
        }

        public bool GetCurrentPersona(out Persona currentPersona)
        {
            currentPersona = CurrentPersona;
            return currentPersona != null;
        }

        public delegate void SuccessGetCurrentPersona(Persona currentPersona);
        public void GetCurrentPersona(SuccessGetCurrentPersona success, GenericError error)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetCurrentPersona(success, error));
        }

        private IEnumerator CoroutineGetCurrentPersona(SuccessGetCurrentPersona success, GenericError error)
        {
            Debug.Log("GetCurrentPersona request started");
            // string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona";
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Get Current Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    CurrentPersona = SerializationHelper.Deserialize<Persona>(request.downloadHandler.text);
                    success?.Invoke(CurrentPersona);
                }
            }
        }

        #endregion GetCurrentPersona

        #region CreatePersona

        public delegate void SuccessCreatePersona();
        public void CreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineCreatePersona(persona, success, error));
        }

        private IEnumerator CoroutineCreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            Debug.Log("CreatePersona request started");
            if (persona.avatarId == null) {
                persona.avatarId = "";
            }
            string jsonPersona = SerializationHelper.Serialize(persona);

            Debug.Log("Persona json: " + jsonPersona);
            
            // string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona";
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            
            Debug.Log("Persona url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";
                
                Debug.Log("Access token: " + currentAccessToken);

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Save Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion CreatePersona

        #region EditPersona

        public delegate void SuccessEditPersona();
        public void EditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineEditPersona(persona, success, error));
        }

        private IEnumerator CoroutineEditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            Debug.Log("Edit Persona request started");
            
            // Fetch the current avatar GUID and add it to the avatarId field of the persona
            if (persona.avatar != null) {
                string personaAvatarTokenURI = Helpers.InternalIPFSURLToHTTP(persona.avatar.tokenURI);
                using (UnityWebRequest tokenURIRequest = UnityWebRequest.Get(personaAvatarTokenURI))
                {
                    yield return tokenURIRequest.SendWebRequest();
                    PrintRequestResult("Avatar tokenURI", tokenURIRequest);
                    TokenURIResponse res = SerializationHelper.Deserialize<List<TokenURIResponse>>(tokenURIRequest.downloadHandler.text)[0];
                    // Debug.Log("GUID: " + res.GUID);
                    // rebuild the avatarId field with the GUID
                    persona.avatarId = persona.avatar.chain + ":" + persona.avatar.contractAddress + ":" +
                                       persona.avatar.tokenId + ":" + res.GUID;
                }
            }

            string jsonPersona = SerializationHelper.Serialize(persona);
            
            Debug.Log("persona json: " + jsonPersona);

            // string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona";
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            Debug.Log("Edit persona url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.method = "PATCH";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", currentAccessToken);

                yield return request.SendWebRequest();
                PrintRequestResult("Save Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion EditPersona

        #region DeletePersona

        public delegate void SuccessDeletePersona();
        public void DeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            // if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineDeletePersona(persona, success, error));
        }

        private IEnumerator CoroutineDeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            Debug.Log("DeletePersona request started");
            // string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona/" + persona.id;
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "DELETE";
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Delete Persona Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }

        #endregion DeletePersona

        #region SetCurrentPersona

        public delegate void SuccessSetCurrentPersona();
        public void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            StartCoroutine(CoroutineSetCurrentPersona(persona, success, error));
        }

        private IEnumerator CoroutineSetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            Debug.Log("Set Current Persona request started");
            // string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "setActivePersona/" + persona.id;
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "setActivePersona/" + persona.id;
            

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "PATCH";
                request.SetRequestHeader("Authorization", currentAccessToken);
                yield return request.SendWebRequest();
                PrintRequestResult("Set Current Persona", request);

                if (RequestError(request))
                {
                    error?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    CurrentPersona = persona;
                    success?.Invoke();
                }
            }
        }

        #endregion SetCurrentPersona

        #region GetAvatars

        public delegate void SuccessAvatars(List<Avatar> avatar);
        public async void AvatarByOwner(string address, SuccessAvatars success, GenericError error)
        {
            Debug.Log("Getting avatars for address: " + address);
            
            Debug.Log("Get Avatars request started");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;
            
            Debug.Log("Requesting avatars from URL: " + url);
            string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, error);
            
            Debug.Log("Avatar response: " + response.ToString());
            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.ToString());
            
            success?.Invoke(avatarResponse.message);
        }
        
        public delegate void SuccessAvatar(Avatar avatar);
        public async void AvatarById(string id, SuccessAvatar success, GenericError error)
        {
            Debug.Log("Getting avatar with id: " + id);
            
            Debug.Log("Get Avatars by id request started");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            Debug.Log("Requesting avatar by id from URL: " + url);
            
            string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, error);
            
            Debug.Log("Avatar by id response: " + response);
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.ToString());

            success?.Invoke(avatarResponse.message);
        }

        #endregion GetAvatars
        
        #region InventoryByOwner

        public delegate void SuccessInventoryByOwner(List<InventoryItem> inventoryItems);
        public async void InventoryByOwner(string address, SuccessInventoryByOwner success, GenericError error)
        {

            Debug.Log("Getting inventory for address: " + address);
                Debug.Log("Inventory By Owner request started");
                // string url = LocalEmergenceServer.Instance.Environment().InventoryURL + "byOwner?address=" + address;
                string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "byOwner?address=" + address;
                Debug.Log("Requesting inventory from URL: " + url);
                string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, error);

                Debug.Log("Inventory response: " + response.ToString());
                InventoryByOwnerResponse inventoryResponse =
                    SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());

                success?.Invoke(inventoryResponse.message.items);
                // StartCoroutine(CoroutineInventoryByOwner(address, success, error));
            
            
        }

        #endregion InventoryByOwner
        
        #region WriteDynamicMetadata
        public delegate void SuccessWriteDynamicMetadata(string response);
        public async void WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, GenericError error)
        {
            metadata = "{\"metadata\": \"" + metadata + "\"}";

            // if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            Debug.Log("Writing dynamic metadata for contract: " + contract);
            Debug.Log("Write dynamic metadata request started");
            // string url = LocalEmergenceServer.Instance.Environment().InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;
            
            Debug.Log("Dynamic metadata url: " + url);
            Debug.Log("updated metadata: " + metadata);
            
            string response = await PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, error, metadata);
            
            Debug.Log("Write dynamic metadata response: " + response.ToString());
            InventoryByOwnerResponse inventoryResponse = SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());
            
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