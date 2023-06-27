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
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;

            WebResponse response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            if(response.IsSuccess == false)
                return;

            GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response.Response);

            success?.Invoke(avatarResponse.message);
        }
    
        public async UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
            
            WebResponse response = await WebRequestService.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            if(response.IsSuccess == false)
                return;
            
            GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.Response);

            success?.Invoke(avatarResponse.message);
        }
    }
}