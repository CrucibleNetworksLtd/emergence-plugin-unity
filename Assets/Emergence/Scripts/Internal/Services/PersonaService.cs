using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class PersonaService : IPersonaService
    {
        public string CurrentAccessToken
        {
            get => currentAccessToken;
            set => currentAccessToken = value;
        }
        private string currentAccessToken = string.Empty;
        public bool HasAccessToken => currentAccessToken.Length > 0;
        
        private readonly ISessionService sessionService;
        public event PersonaUpdated OnCurrentPersonaUpdated;
    
        private Persona cachedPersona;
        public Persona CurrentPersona
        {
            get => cachedPersona;

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

        public async UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "get-access-token";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("GetAccessToken", request);
            if (EmergenceUtils.ProcessRequest<AccessTokenResponse>(request, errorCallback, out var response))
            {
                currentAccessToken = SerializationHelper.Serialize(response.AccessToken, false);
                success?.Invoke(currentAccessToken);
            }
        }
        
        public async UniTask GetPersonas(SuccessPersonas success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            await request.SendWebRequest().ToUniTask();
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
        
        public async UniTask GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", CurrentAccessToken);

            await request.SendWebRequest().ToUniTask();
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

        public async UniTask CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback)
        {
            if (persona.avatarId == null) {
                persona.avatarId = "";
            }
            string jsonPersona = SerializationHelper.Serialize(persona);

            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using UnityWebRequest request = UnityWebRequest.Post(url, string.Empty);
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
            request.uploadHandler.contentType = "application/json";

            request.SetRequestHeader("Authorization", CurrentAccessToken);

            await request.SendWebRequest().ToUniTask();
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

    
        public async UniTask EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            // Fetch the current avatar GUID and add it to the avatarId field of the persona
            if (persona.avatar != null) 
            {
                string personaAvatarTokenURI = Helpers.InternalIPFSURLToHTTP(persona.avatar.tokenURI);
                using UnityWebRequest tokenURIRequest = UnityWebRequest.Get(personaAvatarTokenURI);
                await tokenURIRequest.SendWebRequest().ToUniTask();
                EmergenceUtils.PrintRequestResult("Avatar tokenURI", tokenURIRequest);
                TokenURIResponse res = SerializationHelper.Deserialize<List<TokenURIResponse>>(tokenURIRequest.downloadHandler.text)[0];
                // Debug.Log("GUID: " + res.GUID);
                // rebuild the avatarId field with the GUID
                persona.avatarId = persona.avatar.chain + ":" + persona.avatar.contractAddress + ":" +
                                   persona.avatar.tokenId + ":" + res.GUID;
            }

            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using UnityWebRequest request = UnityWebRequest.Post(url, string.Empty);
            request.method = "PATCH";
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
            request.uploadHandler.contentType = "application/json";

            request.SetRequestHeader("Authorization", CurrentAccessToken);

            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("Save Persona", request);

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

        public async UniTask DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona/" + persona.id;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.method = "DELETE";
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("Delete Persona Request", request);

            if (EmergenceUtils.RequestError(request))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                success?.Invoke();
            }
        }

    
        public async UniTask SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "setActivePersona/" + persona.id;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.method = "PATCH";
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            await request.SendWebRequest().ToUniTask();
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