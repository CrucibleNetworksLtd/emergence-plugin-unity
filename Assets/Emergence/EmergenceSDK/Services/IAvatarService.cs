using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Provides access to the avatar API.
    /// </summary>
    public interface IAvatarService : IEmergenceService
    {
        /// <summary>
        /// Attempts to get the avatars owned by the specified wallet address
        /// </summary>
        /// <param name="address">Owner's wallet address</param>
        /// <param name="ct"><see cref="CancellationToken"/></param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Avatar"/>s owned by the <paramref name="address"/>, wrapped in a <see cref="ServiceResponse{T}"/></returns>
        UniTask<ServiceResponse<List<Avatar>>> AvatarsByOwnerAsync(string address, CancellationToken ct = default);
        
        /// <summary>
        /// Attempts to get the avatars owned by the specified wallet <paramref name="address"/>, with callbacks
        /// </summary>
        /// <param name="address">Owner's wallet address</param>
        /// <param name="success">Delegate of type <see cref="SuccessAvatars"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        /// <param name="cancellationCallback">Delegate of type <see cref="CancellationCallback"/>, called if the operation is cancelled</param>
        /// <param name="ct"><see cref="CancellationToken"/></param>
        UniTask AvatarsByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback, CancellationCallback cancellationCallback = default, CancellationToken ct = default);

        /// <summary>
        /// Attempts to get the avatar matching the given <paramref name="id"/>
        /// </summary>
        /// <param name="id">Requested <see cref="Avatar"/>'s ID</param>
        /// <param name="ct"><see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Avatar"/> matching the <paramref name="id"/>, wrapped in a <see cref="ServiceResponse{T}"/></returns>
        UniTask<ServiceResponse<Avatar>> AvatarByIdAsync(string id, CancellationToken ct = default);
        
        /// <summary>
        /// Attempts to get the avatar matching the given <paramref name="id"/>, with callbacks
        /// </summary>
        /// <param name="id">Requested <see cref="Avatar"/>'s ID</param>
        /// <param name="success">Delegate of type <see cref="SuccessAvatar"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        /// <param name="cancellationCallback">Delegate of type <see cref="CancellationCallback"/>, called if the operation is cancelled</param>
        /// <param name="ct"><see cref="CancellationToken"/></param>
        UniTask AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback, CancellationCallback cancellationCallback = default, CancellationToken ct = default);
    }
}