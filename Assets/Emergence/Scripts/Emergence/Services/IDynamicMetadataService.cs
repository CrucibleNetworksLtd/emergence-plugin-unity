using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Gives access to dynamic metadata. This service is off chain.
    /// </summary>
    public interface IDynamicMetadataService
    {
        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract.
        /// </summary>
        UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);
    }
}