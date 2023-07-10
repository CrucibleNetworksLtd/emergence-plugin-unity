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
            if(response.success)
                success?.Invoke(response.result);
        }
        
        public async UniTask<ServiceResponse<List<Avatar>>> AvatarsByOwnerAsync(string address)
        {
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;

            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<List<Avatar>>(false);

            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.Response);
            return new ServiceResponse<List<Avatar>>(true, avatarResponse.message);
        }
        
        public async UniTask<ServiceResponse<Avatar>> AvatarByIdAsync(string id)
        {
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            
            var response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<Avatar>(false);
            
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.Response);
            return new ServiceResponse<Avatar>(true, avatarResponse.message);
        }

        public async UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback)
        {
            var response = await AvatarByIdAsync(id);
            if(response.success)
                success?.Invoke(response.result);
        }
    }
}