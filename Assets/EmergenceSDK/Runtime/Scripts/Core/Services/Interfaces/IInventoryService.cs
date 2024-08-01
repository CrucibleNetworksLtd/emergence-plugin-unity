using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Inventory;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Service for interacting with the NFT inventory API.
    /// </summary>
    public interface IInventoryService : IEmergenceService
    {
        /// <summary>
        /// Attempts to retrieve the inventory of the given <paramref name="address"/> on the given <paramref name="address"/>.
        /// </summary>
        /// <param name="address">Wallet address</param>
        /// <param name="chain">Chain to check against</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="InventoryItem"/>s wrapped within a <see cref="ServiceResponse{T}"/></returns>
        UniTask<ServiceResponse<List<InventoryItem>>> InventoryByOwnerAsync(string address, InventoryChain chain);
        
        /// <summary>
        /// Attempts to retrieve the inventory of the given <paramref name="address"/> on the given <paramref name="address"/>
        /// Callbacks are called when done
        /// </summary>
        /// <param name="address">Wallet address</param>
        /// <param name="chain">Chain to check against</param>
        /// <param name="success">Delegate of type <see cref="SuccessInventoryByOwner"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
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