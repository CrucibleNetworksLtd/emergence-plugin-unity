using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using UniGLTF;
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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetPersonas(success, error));
        }

        private IEnumerator CoroutineGetPersonas(SuccessPersonas success, GenericError error)
        {
            Debug.Log("GetPersonas request started");
            string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "personas";

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

            private set
            {
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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineGetCurrentPersona(success, error));
        }

        private IEnumerator CoroutineGetCurrentPersona(SuccessGetCurrentPersona success, GenericError error)
        {
            Debug.Log("GetCurrentPersona request started");
            string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona";

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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineCreatePersona(persona, success, error));
        }

        private IEnumerator CoroutineCreatePersona(Persona persona, SuccessCreatePersona success, GenericError error)
        {
            Debug.Log("CreatePersona request started");
            string jsonPersona = SerializationHelper.Serialize(persona);

            string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
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

        #endregion CreatePersona

        #region EditPersona

        public delegate void SuccessEditPersona();
        public void EditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineEditPersona(persona, success, error));
        }

        private IEnumerator CoroutineEditPersona(Persona persona, SuccessEditPersona success, GenericError error)
        {
            Debug.Log("Edit Persona request started");
            string jsonPersona = SerializationHelper.Serialize(persona);

            string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona";
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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineDeletePersona(persona, success, error));
        }

        private IEnumerator CoroutineDeletePersona(Persona persona, SuccessDeletePersona success, GenericError error)
        {
            Debug.Log("DeletePersona request started");
            string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "persona/" + persona.id;

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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            StartCoroutine(CoroutineSetCurrentPersona(persona, success, error));
        }

        private IEnumerator CoroutineSetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, GenericError error)
        {
            Debug.Log("Set Current Persona request started");
            string url = LocalEmergenceServer.Instance.Environment().PersonaURL + "setActivePersona/" + persona.id;

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
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            Debug.Log("Getting avatars for address: " + address);
            
            Debug.Log("Get Avatars request started");
            string url = LocalEmergenceServer.Instance.Environment().AvatarURL + "byOwner?address=" + address;
            Debug.Log("Requesting avatars from URL: " + url);
            string response = await PerformAsyncWebRequest(url, error);
            
            Debug.Log("Avatar response: " + response.ToString());
            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.ToString());
            
            success?.Invoke(avatarResponse.message);
            
            // StartCoroutine(CoroutineGetAvatars(address, success, error));
        }
        
        public async void SwapAvatars(string vrmURL)
        {
            UnityWebRequest request = UnityWebRequest.Get(vrmURL);
            byte[] response = (await request.SendWebRequest()).downloadHandler.data;

            var vrm10 = await Vrm10.LoadBytesAsync(response, true);
            GameObject playerArmature = GameObject.Find("PlayerArmature");
            var originalMesh = playerArmature.GetComponentInChildren<SkinnedMeshRenderer>();
            vrm10.transform.position = playerArmature.transform.position;
            vrm10.transform.rotation = Quaternion.identity;
            vrm10.transform.parent = playerArmature.transform;
            vrm10.name = vrm10.name + "_Imported_v1_0";
            originalMesh.enabled = false;
        }

        // private IEnumerator CoroutineGetAvatars(string address, SuccessAvatars success, GenericError error)
        // {
        //     Debug.Log("Get Avatars request started");
        //     string url = LocalEmergenceServer.Instance.Environment().AvatarURL + "byOwner?address=" + address;
        //     Debug.Log("Requesting avatars from URL: " + url);
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         Debug.Log("AccessToken: " + currentAccessToken);
        //         request.SetRequestHeader("Authorization", currentAccessToken);
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Get Avatars", request);
        //
        //         if (RequestError(request))
        //         {
        //             error?.Invoke(request.error, request.responseCode);
        //         }
        //         else
        //         {
        //             Debug.Log("Avatar response: " + request.downloadHandler.text);
        //             GetAvatarsResponse response = SerializationHelper.Deserialize<GetAvatarsResponse>(request.downloadHandler.text);
        //             Debug.Log("Avatar response: " + response);
        //             success?.Invoke(response.message);
        //         }
        //     }
        // }

        #endregion GetAvatars
        
        #region InventoryByOwner

        public delegate void SuccessInventoryByOwner(List<InventoryItem> inventoryItems);
        public async void InventoryByOwner(string address, SuccessInventoryByOwner success, GenericError error)
        {
          
                if (!LocalEmergenceServer.Instance.CheckEnv())
                {
                    return;
                }

                Debug.Log("Getting inventory for address: " + address);
                Debug.Log("Inventory By Owner request started");
                string url = LocalEmergenceServer.Instance.Environment().InventoryURL + "byOwner?address=" + address;
                Debug.Log("Requesting inventory from URL: " + url);
                string response = await PerformAsyncWebRequest(url, error);

                Debug.Log("Inventory response: " + response.ToString());
                InventoryByOwnerResponse inventoryResponse =
                    SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());

                success?.Invoke(inventoryResponse.message.items);
                // StartCoroutine(CoroutineInventoryByOwner(address, success, error));
            
            
        }

        // private IEnumerator CoroutineInventoryByOwner(string address, SuccessInventoryByOwner success, GenericError error)
        // {
        //     Debug.Log("Inventory By Owner request started");
        //     string url = LocalEmergenceServer.Instance.Environment().InventoryURL + "byOwner?address=" + address;
        //     Debug.Log("Requesting inventory from URL: " + url);
        //
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         Debug.Log("AccessToken: " + currentAccessToken);
        //         request.SetRequestHeader("Authorization", currentAccessToken);
        //         yield return request.SendWebRequest();
        //         PrintRequestResult("Inventory By Owner", request);
        //
        //         if (RequestError(request))
        //         {
        //             error?.Invoke(request.error, request.responseCode);
        //         }
        //         else
        //         {
        //             Debug.Log("Inventory response: " + request.downloadHandler.text);
        //             GetAvatarsResponse response = SerializationHelper.Deserialize<GetAvatarsResponse>(request.downloadHandler.text);
        //             Debug.Log("Inventory response: " + response);
        //             success?.Invoke(new List<InventoryItem>());
        //         }
        //     }
        // }

        #endregion InventoryByOwner
        
        #region WriteDynamicMetadata
        public delegate void SuccessWriteDynamicMetadata(List<InventoryItem> inventoryItems);
        public async void WriteDynamicMetadata(string network, string contract, string tokenId, SuccessWriteDynamicMetadata success, GenericError error)
        {
            if (!LocalEmergenceServer.Instance.CheckEnv()) { return; }
            Debug.Log("Writing dynamic metadata for contract: " + contract);
            Debug.Log("Write dynamic metadata request started");
            string url = LocalEmergenceServer.Instance.Environment().InventoryURL + "byOwner?address=" + address;
            string response = await PerformAsyncWebRequest(url, error);
            
            Debug.Log("Inventory response: " + response.ToString());
            InventoryByOwnerResponse inventoryResponse = SerializationHelper.Deserialize<InventoryByOwnerResponse>(response.ToString());
            
            success?.Invoke(inventoryResponse.message.items);
            // StartCoroutine(CoroutineInventoryByOwner(address, success, error));
        }
        #endregion WriteDynamicMetadata

        #endregion AWS API
    }
}