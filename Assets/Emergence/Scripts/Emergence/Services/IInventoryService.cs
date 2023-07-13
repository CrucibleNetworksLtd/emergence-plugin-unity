using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Service for interacting with the NFT inventory API.
    /// </summary>
    public interface IInventoryService : IEmergenceService
    {
        /// <summary>
        /// Attempts to retrieve the inventory of the given address on the given chain.
        /// </summary>
        UniTask<ServiceResponse<List<InventoryItem>>> InventoryByOwnerAsync(string address, InventoryChain chain);
        /// <summary>
        /// Attempts to retrieve the inventory of the given address on the given chain.
        UniTask InventoryByOwner(string address, InventoryChain chain, SuccessInventoryByOwner success, ErrorCallback errorCallback);
    }
    
    public enum InventoryChain
    {
        AnyCompatible,
        Ethereum,
        Polygon,
        Flow,
        Tezos,
        Solana,
        ImmutableX,
    }

    public static class InventoryKeys
    {
        public static readonly Dictionary<InventoryChain, string> ChainToKey = new Dictionary<InventoryChain, string>()
        {
            {InventoryChain.AnyCompatible, "ETHEREUM,POLYGON,FLOW,TEZOS,SOLANA,IMMUTABLEX"},
            {InventoryChain.Ethereum, "ETHEREUM"},
            {InventoryChain.Polygon, "POLYGON"},
            {InventoryChain.Flow, "FLOW"},
            {InventoryChain.Tezos, "TEZOS"},
            {InventoryChain.Solana, "SOLANA"},
            {InventoryChain.ImmutableX, "IMMUTABLEX"},
        };
    }
}