using Cysharp.Threading.Tasks;
using EmergenceSDK;

public interface IDynamicMetadataService
{
    /// <summary>
    /// Attempts to write dynamic metadata to the specified contract.
    /// </summary>
    UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);
}