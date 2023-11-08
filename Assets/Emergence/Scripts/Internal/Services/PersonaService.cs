using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public event PersonaUpdated OnCurrentPersonaUpdated;
    
        private Persona cachedPersona;
        private Dictionary<string, string> AuthDict => new() { { "deviceId", EmergenceSingleton.Instance.CurrentDeviceId } };

        public Persona CurrentPersona
        {
            get => cachedPersona;

            private set
            {
                if(ObjectEqualityUtil.AreObjectsEqual(cachedPersona, value))
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

        public async UniTask<ServiceResponse<string>> GetAccessTokenAsync()
        {
            string url = StaticConfig.APIBase + "get-access-token";
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError, "", AuthDict);
            if(response.IsSuccess == false)
                return new ServiceResponse<string>(false);
            var accessTokenResponse = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(response.Response);
            currentAccessToken = SerializationHelper.Serialize(accessTokenResponse.message.AccessToken, false);
            return new ServiceResponse<string>(true, currentAccessToken);
        }

        public async UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetAccessTokenAsync();
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in GetAccessToken.", (long)response.Code);
        }
        
        public async UniTask<ServiceResponse<List<Persona>, Persona>> GetPersonasAsync()
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                    return new ServiceResponse<List<Persona>, Persona>(false);
            }
            catch (Exception)
            {
                return new ServiceResponse<List<Persona>, Persona>(false);
            }
            EmergenceUtils.PrintRequestResult("GetPersonas", request);

            if (EmergenceUtils.RequestError(request))
            {
                return new ServiceResponse<List<Persona>, Persona>(false);
            }

            PersonasResponse personasResponse = SerializationHelper.Deserialize<PersonasResponse>(request.downloadHandler.text);
            WebRequestService.CleanupRequest(request);
            CurrentPersona = personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected);
            return new ServiceResponse<List<Persona>, Persona>(true, personasResponse.personas, CurrentPersona);
        }

        public async UniTask GetPersonas(SuccessPersonas success, ErrorCallback errorCallback)
        {
            var response = await GetPersonasAsync();
            if(response.Success)
                success?.Invoke(response.Result0, response.Result1);
            else
                errorCallback?.Invoke("Error in GetPersonas.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<Persona>> GetCurrentPersonaAsync()
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                {
                    WebRequestService.CleanupRequest(request);
                    return new ServiceResponse<Persona>(false);
                }
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<Persona>(false);
            }
            EmergenceUtils.PrintRequestResult("Get Current Persona", request);

            if (EmergenceUtils.RequestError(request))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse<Persona>(false);
            }

            CurrentPersona = SerializationHelper.Deserialize<Persona>(request.downloadHandler.text);
            WebRequestService.CleanupRequest(request);
            return new ServiceResponse<Persona>(true, CurrentPersona);
        }

        public async UniTask GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback)
        {
            var response = await GetCurrentPersonaAsync();
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in GetCurrentPersona.", (long)response.Code);
        }

        public async UniTask<ServiceResponse> CreatePersonaAsync(Persona persona)
        {
            if (persona.avatarId == null)
            {
                persona.avatarId = "";
            }
            else if (persona.avatarId.Length > 3)
            {
                if (persona.avatar != null)
                {
                    var avatarResponse = await UpdateAvatarOnPersonaEdit(persona);
                    if(avatarResponse.Success == false)
                        return new ServiceResponse(false);
                }
            }
            
            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError, jsonPersona, AuthDict);
            if(response.IsSuccess == false)
                return new ServiceResponse(false);
            
            return new ServiceResponse(true);
        }

        public async UniTask CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback)
        {
            var response = await CreatePersonaAsync(persona);
            if(response.Success)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in CreatePersona.", (long)response.Code);
        }

        public async UniTask<ServiceResponse> EditPersonaAsync(Persona persona)
        {
            // Fetch the current avatar GUID and add it to the avatarId field of the persona
            if (persona.avatar != null)
            {
                var avatarResponse = await UpdateAvatarOnPersonaEdit(persona);
                if(avatarResponse.Success == false)
                    return new ServiceResponse(false);
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
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                    return new ServiceResponse(false);
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            if (EmergenceUtils.RequestError(request))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            WebRequestService.CleanupRequest(request);
            CurrentPersona = persona;
            return new ServiceResponse(true);

        }

        public async UniTask EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            var response = await EditPersonaAsync(persona);
            if(response.Success)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in EditPersona.", (long)response.Code);
        }

        private static async UniTask<ServiceResponse> UpdateAvatarOnPersonaEdit(Persona persona)
        {
            string personaAvatarTokenUri = Helpers.InternalIPFSURLToHTTP(persona.avatar.tokenURI);
            UnityWebRequest tokenUriRequest = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, personaAvatarTokenUri, "");
            var response = await WebRequestService.PerformAsyncWebRequest(tokenUriRequest, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse(false);
            TokenURIResponse res = SerializationHelper.Deserialize<List<TokenURIResponse>>(tokenUriRequest.downloadHandler.text)[0];
            WebRequestService.CleanupRequest(tokenUriRequest);
            // rebuild the avatarId field with the GUID
            persona.avatarId = persona.avatar.chain + ":" + persona.avatar.contractAddress + ":" + persona.avatar.tokenId + ":" + res.GUID;
            return new ServiceResponse(true);
        }

        public async UniTask<ServiceResponse> DeletePersonaAsync(Persona persona)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona/" + persona.id;

            using UnityWebRequest request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url);
            request.method = "DELETE";
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                    return new ServiceResponse(false);
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            if (EmergenceUtils.RequestError(request))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            WebRequestService.CleanupRequest(request);
            return new ServiceResponse(true);
        }

        public async UniTask DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback)
        {
            var response = await DeletePersonaAsync(persona);
            if(response.Success)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in DeletePersona.", (long)response.Code);
        }
 
        public async UniTask<ServiceResponse> SetCurrentPersonaAsync(Persona persona)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "setActivePersona/" + persona.id;

            using UnityWebRequest request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.method = "PATCH";
            request.SetRequestHeader("Authorization", CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.IsSuccess == false)
                    return new ServiceResponse(false);
            }
            catch (Exception)
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            if (EmergenceUtils.RequestError(request))
            {
                WebRequestService.CleanupRequest(request);
                return new ServiceResponse(false);
            }

            WebRequestService.CleanupRequest(request);
            CurrentPersona = persona;
            return new ServiceResponse(true);
        }

        public async UniTask SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback)
        {
            var response = await SetCurrentPersonaAsync(persona);
            if(response.Success)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in SetCurrentPersona.", (long)response.Code);
        }

        internal void OnSessionDisconnected()
        {
            CurrentAccessToken = "";
            cachedPersona = null;
        }
    }
}