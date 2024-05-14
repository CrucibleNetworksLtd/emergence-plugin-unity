using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class AvatarService : IAvatarService
    {
        public async UniTask AvatarsByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback, CancellationCallback cancellationCallback, CancellationToken ct)
        {
            try
            {
                var response = await AvatarsByOwnerAsync(address, ct);
                if(response.Successful)
                    success?.Invoke(response.Result1);
                else
                    errorCallback?.Invoke("Error in AvatarsByOwner.", (long)response.Code);
            }
            catch (OperationCanceledException)
            {
                cancellationCallback?.Invoke();
            }
        }
        
        public async UniTask<ServiceResponse<List<Avatar>>> AvatarsByOwnerAsync(string address, CancellationToken ct)
        {
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, ct: ct);
            ct.ThrowIfCancellationRequested();
            
            if(response.Successful == false)
                return new ServiceResponse<List<Avatar>>(false);
            
            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.ResponseText);
            return new ServiceResponse<List<Avatar>>(true, avatarResponse.message);
        }

        public async UniTask<ServiceResponse<Avatar>> AvatarByIdAsync(string id, CancellationToken ct)
        {
            EmergenceLogger.LogInfo($"AvatarByIdAsync: {id}");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, ct: ct);
            if(response.Successful == false)
                return new ServiceResponse<Avatar>(false);
            
            ct.ThrowIfCancellationRequested();
            
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.ResponseText);
            return new ServiceResponse<Avatar>(true, avatarResponse.message);
        }

        public async UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback, CancellationCallback cancellationCallback, CancellationToken ct)
        {
            try
            {
                var response = await AvatarByIdAsync(id, ct: ct);
                if(response.Successful)
                    success?.Invoke(response.Result1);
                else
                    errorCallback?.Invoke("Error in AvatarById.", (long)response.Code);
            }
            catch (OperationCanceledException)
            {
                cancellationCallback?.Invoke();
            }
        }
    }
}