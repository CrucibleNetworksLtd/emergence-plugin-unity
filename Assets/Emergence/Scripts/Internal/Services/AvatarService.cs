using Cysharp.Threading.Tasks;
using EmergenceSDK;
using EmergenceSDK.Emergence.Scripts.Emergence.Services;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarService : IAvatarService
{
    public async UniTask AvatarByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback)
    {
        Debug.Log("Getting avatars for address: " + address);

        string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "byOwner?address=" + address;

        Debug.Log("Requesting avatars from URL: " + url);
        string response = await Services.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);

        Debug.Log("Avatar response: " + response);
        GetAvatarsResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarsResponse>(response);

        success?.Invoke(avatarResponse.message);
    }
    
    public async UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback)
    {
        Debug.Log("Getting avatar with id: " + id);
            
        string url = EmergenceSingleton.Instance.Configuration.AvatarURL + "id?id=" + id;
        Debug.Log("Requesting avatar by id from URL: " + url);
            
        string response = await Services.PerformAsyncWebRequest(url, UnityWebRequest.kHttpVerbGET, errorCallback);
            
        Debug.Log("Avatar by id response: " + response);
        GetAvatarResponse avatarResponse = SerializationHelper.Deserialize<GetAvatarResponse>(response.ToString());

        success?.Invoke(avatarResponse.message);
    }
}