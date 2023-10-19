using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Gives access to dynamic metadata.
    /// </summary>
    public interface IDynamicMetadataService : IEmergenceService
    {
        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must already be dynamic metadata on the object.
        /// </summary>
        UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must already be dynamic metadata on the object.
        /// </summary>
        UniTask<ServiceResponse<string>> WriteDynamicMetadataAsync(string network, string contract, string tokenId, string metadata);
        
        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must not be dynamic metadata on the object.
        /// </summary>
        UniTask WriteNewDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must not be dynamic metadata on the object.
        /// </summary>
        UniTask<ServiceResponse<string>> WriteNewDynamicMetadataAsync(string network, string contract, string tokenId, string metadata);
    }
}