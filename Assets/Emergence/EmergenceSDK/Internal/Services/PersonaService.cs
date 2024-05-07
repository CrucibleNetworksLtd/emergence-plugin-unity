using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class PersonaService : IPersonaServiceInternal, IPersonaService, IDisconnectableService
    {
        public readonly ISessionServiceInternal SessionServiceInternal;
        public PersonaService(ISessionServiceInternal sessionServiceInternal)
        {
            SessionServiceInternal = sessionServiceInternal;
        }
        
        public event PersonaUpdated OnCurrentPersonaUpdated;
    
        private Persona cachedPersona;

        private Persona CachedPersona
        {
            get => cachedPersona;

            set
            {
                if(ObjectEqualityUtil.AreObjectsEqual(cachedPersona, value))
                    return;

                cachedPersona = value;
                OnCurrentPersonaUpdated?.Invoke(cachedPersona);
            }

        }
        
        public bool GetCachedPersona(out Persona currentPersona)
        {
            return (currentPersona = CachedPersona) != null;
        }


        
        public async UniTask<ServiceResponse<List<Persona>, Persona>> GetPersonasAsync()
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.SetRequestHeader("Authorization", SessionServiceInternal.CurrentAccessToken);
            try
            {
                var response  = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
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
            CachedPersona = personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected);
            return new ServiceResponse<List<Persona>, Persona>(true, personasResponse.personas, CachedPersona);
        }

        public async UniTask GetPersonas(SuccessPersonas success, ErrorCallback errorCallback)
        {
            var response = await GetPersonasAsync();
            if(response.Successful)
                success?.Invoke(response.Result1, response.Result2);
            else
                errorCallback?.Invoke("Error in GetPersonas.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<Persona>> GetCurrentPersonaAsync()
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.SetRequestHeader("Authorization", SessionServiceInternal.CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
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

            CachedPersona = SerializationHelper.Deserialize<Persona>(request.downloadHandler.text);
            WebRequestService.CleanupRequest(request);
            return new ServiceResponse<Persona>(true, CachedPersona);
        }

        public async UniTask GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback)
        {
            var response = await GetCurrentPersonaAsync();
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in GetCurrentPersona.", (long)response.Code);
        }

        public async UniTask<ServiceResponse> CreatePersonaAsync(Persona persona)
        {
            await UpdateAvatarOnPersonaEdit(persona);
            
            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            var headers = new Dictionary<string, string> { { "deviceId", EmergenceSingleton.Instance.CurrentDeviceId } };
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError, jsonPersona, headers);
            if(response.Successful == false)
                return new ServiceResponse(false);
            
            return new ServiceResponse(true);
        }

        public async UniTask CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback)
        {
            var response = await CreatePersonaAsync(persona);
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in CreatePersona.", (long)response.Code);
        }

        public async UniTask<ServiceResponse> EditPersonaAsync(Persona persona)
        {
            await UpdateAvatarOnPersonaEdit(persona);

            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

            using UnityWebRequest request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, url, "");
            request.method = "PATCH";
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPersona));
            request.uploadHandler.contentType = "application/json";

            request.SetRequestHeader("Authorization", SessionServiceInternal.CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
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
            CachedPersona = persona;
            return new ServiceResponse(true);

        }

        public async UniTask EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            var response = await EditPersonaAsync(persona);
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in EditPersona.", (long)response.Code);
        }

        private static async UniTask<ServiceResponse> UpdateAvatarOnPersonaEdit(Persona persona)
        {
            var isAvatarValid = 
                persona.avatar is { chain: not null } // Pattern matching syntax, matches the pattern where avatar has a not null chain field, also fails if avatar is null
                && persona.avatar.chain.Trim() != ""
                && persona.avatar.contractAddress.Trim() != ""
                && persona.avatar.tokenId.Trim() != ""
                ;
            
            if (!isAvatarValid)
            {
                return new ServiceResponse(false);
            }
                
            string personaAvatarTokenUri = Helpers.InternalIPFSURLToHTTP(persona.avatar.tokenURI);
            UnityWebRequest tokenUriRequest = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, personaAvatarTokenUri, "");
            var response = await WebRequestService.PerformAsyncWebRequest(tokenUriRequest, EmergenceLogger.LogError);
            if(response.Successful == false)
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
            request.SetRequestHeader("Authorization", SessionServiceInternal.CurrentAccessToken);
            
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
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
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in DeletePersona.", (long)response.Code);
        }
 
        public async UniTask<ServiceResponse> SetCurrentPersonaAsync(Persona persona)
        {
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "setActivePersona/" + persona.id;

            using UnityWebRequest request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbGET, url, "");
            request.method = "PATCH";
            request.SetRequestHeader("Authorization", SessionServiceInternal.CurrentAccessToken);
            try
            {
                var response = await WebRequestService.PerformAsyncWebRequest(request, EmergenceLogger.LogError);
                if(response.Successful == false)
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
            CachedPersona = persona;
            return new ServiceResponse(true);
        }

        public async UniTask SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback)
        {
            var response = await SetCurrentPersonaAsync(persona);
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in SetCurrentPersona.", (long)response.Code);
        }

        public void HandleDisconnection()
        {
            cachedPersona = null;
        }
    }
}