using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Emergence.Scripts.Emergence.Services
{
    public interface IAvatarService
    {
        /// <summary>
        /// Attempts to get the avatars for the given address. If successful, the success callback will be called with the avatars.
        /// </summary>
        UniTask AvatarByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to get the avatar for the given id. If successful, the success callback will be called with the avatar.
        /// </summary>
        UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback);
    }
}