using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Provides access to the avatar API.
    /// </summary>
    public interface IAvatarService : IEmergenceService
    {
        /// <summary>
        /// Attempts to get the avatars for the given address. If successful, the success callback will be called with the avatars.
        /// </summary>
        UniTask AvatarsByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to get the avatar for the given id. If successful, the success callback will be called with the avatar.
        /// </summary>
        UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback);
    }
}