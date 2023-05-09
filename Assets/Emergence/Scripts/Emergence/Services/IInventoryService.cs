using Cysharp.Threading.Tasks;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the NFT inventory API.
    /// </summary>
    public interface IInventoryService : IEmergenceService
    {
        /// <summary>
        /// Attempts to get the inventory for the given address. If successful, the success callback will be called with the inventory.
        /// <remarks>We currently support Ethereum, Polygon, Flow, Tezos, Solana and ImmutableX</remarks>
        /// </summary>
        UniTask InventoryByOwner(string address, SuccessInventoryByOwner success, ErrorCallback errorCallback);
    }
}