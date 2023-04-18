using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Gives access to dynamic metadata.
    /// </summary>
    public interface IDynamicMetadataService : IEmergenceService
    {
        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract.
        /// </summary>
        UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);
    }
}