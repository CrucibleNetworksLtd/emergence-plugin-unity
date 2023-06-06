using System.Collections.Generic;
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
        /// <remarks>We currently support the main nets for: Ethereum, Polygon, Flow, Tezos, Solana and ImmutableX</remarks>
        /// </summary>
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