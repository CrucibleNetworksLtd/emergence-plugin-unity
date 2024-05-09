using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Exceptions;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class PersonaService : IPersonaServiceInternal, IPersonaService, ISessionConnectableService
    {
        private readonly ISessionServiceInternal _sessionServiceInternal;
        private readonly ISessionService _sessionService;
        public PersonaService(ISessionServiceInternal sessionServiceInternal)
        {
            _sessionServiceInternal = sessionServiceInternal;
            _sessionService = (ISessionService)_sessionServiceInternal;
        }
        
        public event PersonaUpdated OnCurrentPersonaUpdated;
    
        private Persona _cachedPersona;
        private Persona CachedPersona
        {
            get => _cachedPersona;

            set
            {
                if(ObjectEqualityUtil.AreObjectsEqual(_cachedPersona, value))
                    return;

                _cachedPersona = value;
                OnCurrentPersonaUpdated?.Invoke(_cachedPersona);
            }

        }
        
        public bool GetCachedPersona(out Persona currentPersona)
        {
            return (currentPersona = CachedPersona) != null;
        }


        
        public async UniTask<ServiceResponse<List<Persona>, Persona>> GetPersonasAsync()
        {
            if (_sessionService.HasLoginSetting(LoginSettings.DisableEmergenceAccessToken)) { throw new EmergenceAccessTokenDisabledException(); }
            
            try
            { 
                var url = EmergenceSingleton.Instance.Configuration.PersonaURL + "personas";
                var response  = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, headers: _sessionServiceInternal.EmergenceAccessTokenHeader);
                if(response.Successful == false)
                    return new ServiceResponse<List<Persona>, Persona>(false);
                
                if (EmergenceUtils.RequestError(response.Request))
                {
                    return new ServiceResponse<List<Persona>, Persona>(false);
                }

                PersonasResponse personasResponse = SerializationHelper.Deserialize<PersonasResponse>(response.ResponseText);
                CachedPersona = personasResponse.personas.FirstOrDefault(p => p.id == personasResponse.selected);
                return new ServiceResponse<List<Persona>, Persona>(true, personasResponse.personas, CachedPersona);
            }
            catch (Exception)
            {
                return new ServiceResponse<List<Persona>, Persona>(false);
            }
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
            if (_sessionService.HasLoginSetting(LoginSettings.DisableEmergenceAccessToken)) {  throw new EmergenceAccessTokenDisabledException(); }

            try
            {
                string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
                var response  = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, headers: _sessionServiceInternal.EmergenceAccessTokenHeader);
                if(response.Successful == false)
                {
                    return new ServiceResponse<Persona>(false);
                }
                
                if (EmergenceUtils.RequestError(response.Request))
                {
                    return new ServiceResponse<Persona>(false);
                }

                CachedPersona = SerializationHelper.Deserialize<Persona>(response.ResponseText);
                return new ServiceResponse<Persona>(true, CachedPersona);
            }
            catch (Exception)
            {
                return new ServiceResponse<Persona>(false);
            }
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
            if (_sessionService.HasLoginSetting(LoginSettings.DisableEmergenceAccessToken)) {  throw new EmergenceAccessTokenDisabledException(); }

            await UpdateAvatarOnPersonaEdit(persona);
            
            string jsonPersona = SerializationHelper.Serialize(persona);
            string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";
            var headers = EmergenceSingleton.DeviceIdHeader;
            headers.Add("Authorization", _sessionServiceInternal.EmergenceAccessToken);

            
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, jsonPersona, headers);
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

        public async UniTask EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback)
        {
            var response = await EditPersonaAsync(persona);
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in EditPersona.", (long)response.Code);
        }

        public async UniTask<ServiceResponse> EditPersonaAsync(Persona persona)
        {
            if (_sessionService.HasLoginSetting(LoginSettings.DisableEmergenceAccessToken)) {  throw new EmergenceAccessTokenDisabledException(); }

            try
            {
                await UpdateAvatarOnPersonaEdit(persona);

                string jsonPersona = SerializationHelper.Serialize(persona);
                string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona";

                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Patch, url, jsonPersona, _sessionServiceInternal.EmergenceAccessTokenHeader);
                if(response.Successful == false)
                    return new ServiceResponse(false);
                
                if (EmergenceUtils.RequestError(response.Request))
                {
                    return new ServiceResponse(false);
                }

                CachedPersona = persona;
                return new ServiceResponse(true);
            }
            catch (Exception)
            {
                return new ServiceResponse(false);
            }
        }

        private static async UniTask<ServiceResponse> UpdateAvatarOnPersonaEdit(Persona persona)
        {
            try
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
                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, personaAvatarTokenUri);
                if(response.Successful == false)
                    return new ServiceResponse(false);
                
                if (EmergenceUtils.RequestError(response.Request))
                {
                    return new ServiceResponse(false);
                }
                
                TokenURIResponse res = SerializationHelper.Deserialize<List<TokenURIResponse>>(response.ResponseText)[0];
                // rebuild the avatarId field with the GUID
                persona.avatarId = persona.avatar.chain + ":" + persona.avatar.contractAddress + ":" + persona.avatar.tokenId + ":" + res.GUID;
                
                return new ServiceResponse(true);
            }
            catch (Exception)
            {
                return new ServiceResponse(false);
            }
        }

        public async UniTask<ServiceResponse> DeletePersonaAsync(Persona persona)
        {
            if (_sessionService.HasLoginSetting(LoginSettings.DisableEmergenceAccessToken)) {  throw new EmergenceAccessTokenDisabledException(); }

            try
            {
                string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "persona/" + persona.id;

                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Delete, url, headers: _sessionServiceInternal.EmergenceAccessTokenHeader);
                if(response.Successful == false)
                    return new ServiceResponse(false);
                
                if (EmergenceUtils.RequestError(response.Request))
                {
                    return new ServiceResponse(false);
                }

                return new ServiceResponse(true);
            }
            catch (Exception)
            {
                return new ServiceResponse(false);
            }
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
            if (_sessionService.HasLoginSetting(LoginSettings.DisableEmergenceAccessToken)) {  throw new EmergenceAccessTokenDisabledException(); }
            
            try
            {
                string url = EmergenceSingleton.Instance.Configuration.PersonaURL + "setActivePersona/" + persona.id;

                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Patch, url, headers: _sessionServiceInternal.EmergenceAccessTokenHeader);
                if(response.Successful == false)
                    return new ServiceResponse(false);
                
                if (EmergenceUtils.RequestError(response.Request))
                {
                    return new ServiceResponse(false);
                }

                CachedPersona = persona;
                return new ServiceResponse(true);
            }
            catch (Exception)
            {
                return new ServiceResponse(false);
            }
        }

        public async UniTask SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback)
        {
            var response = await SetCurrentPersonaAsync(persona);
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in SetCurrentPersona.", (long)response.Code);
        }

        public void HandleDisconnection(ISessionService sessionService)
        {
            _cachedPersona = null;
        }

        public void HandleConnection(ISessionService sessionService)
        {
            _cachedPersona = null;
        }
    }
}