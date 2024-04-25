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
                if(response.Success)
                    success?.Invoke(response.Result);
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

            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError, ct: ct);
            if(response.IsSuccess == false)
                return new ServiceResponse<List<Avatar>>(false);

            ct.ThrowIfCancellationRequested();
            
            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.Response);
            return new ServiceResponse<List<Avatar>>(true, avatarResponse.message);
        }

        public async UniTask<ServiceResponse<Avatar>> AvatarByIdAsync(string id, CancellationToken ct)
        {
            EmergenceLogger.LogInfo($"AvatarByIdAsync: {id}");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError, ct: ct);
            if(response.IsSuccess == false)
                return new ServiceResponse<Avatar>(false);
            
            ct.ThrowIfCancellationRequested();
            
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.Response);
            return new ServiceResponse<Avatar>(true, avatarResponse.message);
        }

        public async UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback, CancellationCallback cancellationCallback, CancellationToken ct)
        {
            try
            {
                var response = await AvatarByIdAsync(id, ct: ct);
                if(response.Success)
                    success?.Invoke(response.Result);
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