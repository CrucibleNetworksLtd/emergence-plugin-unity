using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse.Types;
using EmergenceSDK.Runtime.Futureverse.Types.Exceptions;
using EmergenceSDK.Runtime.Futureverse.Types.Responses;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Exceptions;
using EmergenceSDK.Runtime.Types.Responses;

namespace EmergenceSDK.Runtime.Futureverse.Services
{
    public interface IFutureverseService : IEmergenceService
    {
        public FuturepassInformationResponse CurrentFuturepassInformation { get; }
        
        /// <summary>
        /// Get the API Url for the asset register API
        /// </summary>
        /// <returns>The API Url for the Asset Register API</returns>
        public string GetArApiUrl();
        
        /// <summary>
        /// Get the API url for the futurepass API
        /// </summary>
        /// <returns>The API url for the Futurepass API</returns>
        public string GetFuturepassApiUrl();
        
        /// <summary>
        /// Get the Futurepass linked to the current wallet
        /// </summary>
        /// <returns>A <see cref="ServiceResponse{T}"/> object wrapping a <see cref="LinkedFuturepassResponse"/></returns>
        /// <exception cref="InvalidWalletException">Thrown if there is no currently connected wallet</exception>
        UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassAsync();
        
        /// <summary>
        /// Get the Futurepass information for the passed Futurepass
        /// </summary>
        /// <param name="futurepass">Futurepass address</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object wrapping a <see cref="FuturepassInformationResponse"/></returns>
        UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturepassInformationAsync(string futurepass);
        
        /// <summary>
        /// (OBSOLETE) Retrieve the asset tree for the Token ID and Collection ID
        /// </summary>
        /// <param name="tokenId">Token ID for tree retrieval</param>
        /// <param name="collectionId">Collection ID for tree retrieval</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="AssetTreePathLegacy"/> objects</returns>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        /// <exception cref="FutureverseInvalidJsonStructureException">Thrown if the asset tree response doesn't match the expected JSON structure</exception>
        [Obsolete]
        UniTask<List<AssetTreePathLegacy>> GetAssetTreeAsyncLegacy(string tokenId, string collectionId);
        
        /// <summary>
        /// Retrieve the asset tree for the Token ID and Collection ID
        /// </summary>
        /// <param name="tokenId">Token ID for tree retrieval</param>
        /// <param name="collectionId">Collection ID for tree retrieval</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="AssetTreePath"/> objects</returns>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        UniTask<List<AssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId);

        /// <summary>
        /// Send an ARTM from the current wallet
        /// </summary>
        /// <param name="message">The message for the ARTM to send</param>
        /// <param name="artmOperations">A list of FutureverseArtmOperation structs</param>
        /// <param name="retrieveStatus">If true, this method will call <see cref="GetArtmStatusAsync"/> with the default parameters to retrieve the status for the transaction.</param>
        /// <returns>A <see cref="ArtmTransactionResponse"/> object, containing the transaction hash. If the transaction status was retrieved it will also be present.</returns>
        /// <exception cref="InvalidWalletException">Thrown if there is no currently connected wallet</exception>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        /// <exception cref="SignMessageFailedException">Thrown if the needed sign message request fails</exception>
        Task<ArtmTransactionResponse> SendArtmAsync(string message, List<ArtmOperation> artmOperations, bool retrieveStatus = true);
        
        /// <summary>
        /// Get the status of a ARTM transaction by its hash
        /// </summary>
        /// <param name="transactionHash">Hash of the transaction to check</param>
        /// <param name="initialDelay">How long to wait before making the first request, in milliseconds</param>
        /// <param name="refetchInterval">How long to wait between each subsequent request, in milliseconds</param>
        /// <param name="maxRetries">How many times to try fetching the status should it be pending</param>
        /// <returns><see cref="ArtmStatus"/> enum representing the transaction status</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the transaction status is unexpected</exception>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        UniTask<ArtmStatus> GetArtmStatusAsync(string transactionHash, int initialDelay = 1000, int refetchInterval = 5000, int maxRetries = 3);

        /// <summary>
        /// Gets the futurepass inventory of the given addresses, filtered by the given collections.
        /// </summary>
        /// <param name="addresses">A list of addresses to get the futurepass inventory of - just the address, no blockchain information.</param>
        /// <param name="collectionIds">A list of collections to filter the inventory by - these should be in the format [chainID]:[chainType]:[Contract address], e.g. "5:evm:0x1cac32d9893deca7769a2e64edc186163125d43b"</param>
        /// <returns><see cref="ArtmStatus"/> enum representing the transaction status</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the transaction status is unexpected</exception>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        UniTask<ServiceResponse<InventoryResponse>> GetFuturepassInventoryByCollectionAndOwnerAsync(List<string> addresses, List<string> collectionIds);
    }
}