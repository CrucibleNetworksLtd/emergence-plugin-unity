using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Internal.Services
{
    public class PersonaService : MonoBehaviour, IPersonaService
    {
        public event PersonaUpdated OnCurrentPersonaUpdated;
    
        private Persona cachedPersona;
        public Persona CurrentPersona
        {
            get
            {
                return cachedPersona;
            }

            private set 
            {
                if (cachedPersona != null && cachedPersona.id.Equals(value.id)) 
                    return; // don't do anything if the new persona is the same as the cached one)
                cachedPersona = value;
                OnCurrentPersonaUpdated?.Invoke(cachedPersona);
            }
        }
    
        public bool GetCurrentPersona(out Persona currentPersona)
        {
            currentPersona = CurrentPersona;
            return currentPersona != null;
        }
    
        public void GetPersonas(SuccessPersonas success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineGetPersonas(success, errorCallback));
        }
    
        private IEnumerator CoroutineGetPersonas(SuccessPersonas success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("GetPersonas", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    PersonasResponse personasResponse = SerializationHelper.Deserialize<PersonasResponse>(request.downloadHandler.text);
                    CurrentPersona = personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected);
                    success?.Invoke(personasResponse.personas, CurrentPersona);
                }
            }
        }
    
        public void GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineGetCurrentPersona(success, errorCallback));
        }

        private IEnumerator CoroutineGetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Get Current Persona", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    CurrentPersona = SerializationHelper.Deserialize<Persona>(request.downloadHandler.text);
                    success?.Invoke(CurrentPersona);
                }
            }
        }
    
        public void CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineCreatePersona(persona, success, errorCallback));
        }

        private IEnumerator CoroutineCreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback)
        {
            if (persona.avatarId == null) {
                persona.avatarId = "";
            }
            string jsonPersona = SerializationHelper.Serialize(persona);

            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
        
            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Save Persona", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }
    
        public void EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineEditPersona(persona, success, errorCallback));
        }

        private IEnumerator CoroutineEditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            // Fetch the current avatar GUID and add it to the avatarId field of the persona
            if (persona.avatar != null) {
                string personaAvatarTokenURI = Helpers.InternalIPFSURLToHTTP(persona.avatar.tokenURI);
                using (UnityWebRequest tokenURIRequest = UnityWebRequest.Get(personaAvatarTokenURI))
                {
                    yield return tokenURIRequest.SendWebRequest();
                    EmergenceUtils.PrintRequestResult("Avatar tokenURI", tokenURIRequest);
                    TokenURIResponse res = SerializationHelper.Deserialize<List<TokenURIResponse>>(tokenURIRequest.downloadHandler.text)[0];
                    // Debug.Log("GUID: " + res.GUID);
                    // rebuild the avatarId field with the GUID
                    persona.avatarId = persona.avatar.chain + ":" + persona.avatar.contractAddress + ":" +
                                       persona.avatar.tokenId + ":" + res.GUID;
                }
            }

            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using (UnityWebRequest request = UnityWebRequest.Post(url, string.Empty))
            {
                request.method = "PATCH";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
                request.uploadHandler.contentType = "application/json";

                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);

                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Save Persona", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }
    
        public void DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineDeletePersona(persona, success, errorCallback));
        }

        private IEnumerator CoroutineDeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "DELETE";
                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Delete Persona Persona", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    success?.Invoke();
                }
            }
        }
    
        public void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback)
        {
            StartCoroutine(CoroutineSetCurrentPersona(persona, success, errorCallback));
        }

        private IEnumerator CoroutineSetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "setActivePersona/" + persona.id;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.method = "PATCH";
                request.SetRequestHeader("Authorization", EmergenceServices.Instance.CurrentAccessToken);
                yield return request.SendWebRequest();
                EmergenceUtils.PrintRequestResult("Set Current Persona", request);

                if (EmergenceUtils.RequestError(request))
                {
                    errorCallback?.Invoke(request.error, request.responseCode);
                }
                else
                {
                    CurrentPersona = persona;
                    success?.Invoke();
                }
            }
        }
    }
}