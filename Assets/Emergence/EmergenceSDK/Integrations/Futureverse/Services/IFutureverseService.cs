using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Exceptions;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Exceptions;
using EmergenceSDK.Types.Inventory;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Integrations.Futureverse.Services
{
    public interface IFutureverseService : IEmergenceService
    {
        /// <summary>
        /// This property is set to true when Futureverse is enabled
        /// </summary>
        bool UsingFutureverse { get; }

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
        /// Retrieve the asset tree for the Token ID and Collection ID
        /// </summary>
        /// <param name="tokenId">Token ID for tree retrieval</param>
        /// <param name="collectionId">Collection ID for tree retrieval</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="FutureverseAssetTreePath"/> objects</returns>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        /// <exception cref="FutureverseInvalidJsonStructureException">Thrown if the asset tree response doesn't match the expected JSON structure</exception>
        UniTask<List<FutureverseAssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId);

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
        Task<ArtmTransactionResponse> SendArtmAsync(string message, List<FutureverseArtmOperation> artmOperations, bool retrieveStatus = true);
        
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
    }
}