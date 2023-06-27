using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public event PersonaUpdated OnCurrentPersonaUpdated;
    
        private Persona cachedPersona;
        private Dictionary<string, string> AuthDict => new Dictionary<string, string>() { { "deviceId", EmergenceSingleton.Instance.CurrentDeviceId } };

        public Persona CurrentPersona
        {
            get => cachedPersona;

            private set 
            {
                if (cachedPersona?.id == value?.id)
                    return;

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
            string url = StaticConfig.APIBase + "get-access-token";
            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback, "", AuthDict);
            if(response.IsSuccess == false)
                return;
            var accessTokenResponse = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(response.Response);
            currentAccessToken = SerializationHelper.Serialize(accessTokenResponse.message.AccessToken, false);
            success?.Invoke(currentAccessToken);
        }
        
        public async UniTask GetPersonas(SuccessPersonas success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
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
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
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
            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbPOST, errorCallback, jsonPersona, AuthDict);
            if(response.IsSuccess == false)
                return;
            
            success?.Invoke();
        }

    
        public async UniTask EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            // Fetch the current avatar GUID and add it to the avatarId field of the persona
            if (persona.avatar != null)
            {
                await UpdateAvatarOnPersonaEdit(persona, errorCallback);
            }

            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using UnityWebRequest request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, "");
            request.method = "PATCH";
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
            request.uploadHandler.contentType = "application/json";

            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }

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

        private static async UniTask UpdateAvatarOnPersonaEdit(Persona persona, ErrorCallback errorCallback)
        {
            string personaAvatarTokenUri = Helpers.InternalIPFSURLToHTTP(persona.avatar.tokenURI);
            UnityWebRequest tokenUriRequest = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, personaAvatarTokenUri, "");
            var response = await WebRequestService.PerformAsyncWebRequest(tokenUriRequest, errorCallback);
            if(response.IsSuccess == false)
                return;
            TokenURIResponse res = SerializationHelper.Deserialize<List<TokenURIResponse>>(tokenUriRequest.downloadHandler.text)[0];
            // rebuild the avatarId field with the GUID
            persona.avatarId = persona.avatar.chain + ":" + persona.avatar.contractAddress + ":" +
                               persona.avatar.tokenId + ":" + res.GUID;
        }

        public async UniTask DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona/" + persona.id;

            using UnityWebRequest request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.method = "DELETE";
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }

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

            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.method = "PATCH";
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, errorCallback);
                if(response.IsSuccess == false)
                    return;
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
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

        internal void OnSessionDisconnected()
        {
            CurrentAccessToken = "";
            cachedPersona = null;
        }
    }
}