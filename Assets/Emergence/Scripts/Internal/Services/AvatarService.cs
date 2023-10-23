using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class AvatarService : IAvatarService
    {
        public async UniTask AvatarsByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback)
        {
            var response = await AvatarsByOwnerAsync(address);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in AvatarsByOwner.", (long)response.Code);
        }
        
        public async UniTask<ServiceResponse<List<Avatar>>> AvatarsByOwnerAsync(string address)
        {
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;

            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<List<Avatar>>(false);

            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.Response);
            return new ServiceResponse<List<Avatar>>(true, avatarResponse.message);
        }
        
        public async UniTask<ServiceResponse<Avatar>> AvatarByIdAsync(string id)
        {
            EmergenceLogger.LogInfo($"AvatarByIdAsync: {id}");
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<Avatar>(false);
            
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.Response);
            return new ServiceResponse<Avatar>(true, avatarResponse.message);
        }

        public async UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback)
        {
            var response = await AvatarByIdAsync(id);
            if(response.Success)
                success?.Invoke(response.Result);
            else
                errorCallback?.Invoke("Error in AvatarById.", (long)response.Code);
        }
    }
}