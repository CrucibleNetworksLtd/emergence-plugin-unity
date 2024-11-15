using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse.Internal.Services;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Futureverse.Types;
using EmergenceSDK.Runtime.Futureverse.Types.Exceptions;
using EmergenceSDK.Runtime.Futureverse.Types.Responses;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Exceptions;
using EmergenceSDK.Runtime.Types.Responses;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Runtime.Futureverse.Internal
{
    internal class FutureverseService : IFutureverseService, IFutureverseServiceInternal, ISessionConnectableService
    {
        public FuturepassInformationResponse CurrentFuturepassInformation { get; set; }

        private readonly IWalletService walletService;

        public FutureverseService(IWalletService walletService)
        {
            this.walletService = walletService;
        }

        public string GetArApiUrl()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            return "https://ar-api.futureverse.app/graphql";
#else
            return EmergenceSingleton.Instance.Environment switch
            {
                EmergenceEnvironment.Production => "https://ar-api.futureverse.app/graphql",
                EmergenceEnvironment.Development => "https://ar-api.futureverse.dev/graphql",
                EmergenceEnvironment.Staging => "https://ar-api.futureverse.cloud/graphql",
                _ => throw new ArgumentOutOfRangeException()
            };
#endif
        }
        
        public string GetFuturepassApiUrl()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            return "https://account-indexer.pass.online/api/v1/";
#else
            return FutureverseSingleton.Instance.Environment switch
            {
                EmergenceEnvironment.Production => "https://account-indexer.pass.online/api/v1/",
                EmergenceEnvironment.Development => "https://account-indexer.passonline.dev/api/v1/",
                EmergenceEnvironment.Staging => "https://account-indexer.passonline.dev/api/v1/",
                _ => throw new ArgumentOutOfRangeException()
            };
#endif
        }
        
        public async UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassAsync()
        {
            if (!walletService.IsValidWallet)
            {
                throw new InvalidWalletException();
            }
            
            var url = $"{GetFuturepassApiUrl()}linked-futurepass?eoa={EmergenceSingleton.Instance.Configuration.Chain.ChainID}:EVM:{walletService.WalletAddress}";

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
            
            if (!response.Successful)
                return new ServiceResponse<LinkedFuturepassResponse>(response);

            LinkedFuturepassResponse fpResponse =
                SerializationHelper.Deserialize<LinkedFuturepassResponse>(response.ResponseText);

            return new ServiceResponse<LinkedFuturepassResponse>(response, true, fpResponse);
        }

        public async UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassAsync(string eoa)
        {
            var url = $"{GetFuturepassApiUrl()}linked-futurepass?eoa={EmergenceSingleton.Instance.Configuration.Chain.ChainID}:EVM:{eoa}";

            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
            
            if (!response.Successful)
                return new ServiceResponse<LinkedFuturepassResponse>(response);

            LinkedFuturepassResponse fpResponse =
                SerializationHelper.Deserialize<LinkedFuturepassResponse>(response.ResponseText);

            return new ServiceResponse<LinkedFuturepassResponse>(response, true, fpResponse);
        }

        public async UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturepassInformationAsync(string futurepass)
        {
            var url = $"{GetFuturepassApiUrl()}linked-eoa?futurepass={futurepass}";
            
            var response = await WebRequestService.SendAsyncWebRequest(
                RequestMethod.Get,
                url,
                timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);

            
            if (!response.Successful)
                return new ServiceResponse<FuturepassInformationResponse>(response, false);

            FuturepassInformationResponse fpResponse =
                SerializationHelper.Deserialize<FuturepassInformationResponse>(response.ResponseText);
            return new ServiceResponse<FuturepassInformationResponse>(response, true, fpResponse);
        }
        
        public async UniTask<List<AssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId)
        {
            var body = BuildGetAssetTreeRequestBody(tokenId, collectionId);
            var response = await WebRequestService.SendAsyncWebRequest(
                RequestMethod.Post,
                GetArApiUrl(),
                body,
                timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
            
            if (!IsArResponseValid(response, out var jObject))
            {
                throw BuildAssetRegisterException(response, jObject);
            }
            
            return DeserializeGetAssetTreeResponseJson(response.ResponseText);
        }

        public List<AssetTreePath> DeserializeGetAssetTreeResponseJson(string json)
        {
            return SerializationHelper.Deserialize<List<AssetTreePath>>(SerializationHelper.Parse(json).SelectToken("data.asset.assetTree.data.@graph"));
        }

        private static string BuildGetAssetTreeRequestBody(string tokenId, string collectionId)
        {
            var requestBody = new
            {
                query = "query Asset($tokenId: String!, $collectionId: CollectionId!) { asset(tokenId: $tokenId, collectionId: $collectionId) { assetTree { data } } }",
                variables = new
                {
                    tokenId, collectionId
                }
            };
            return SerializationHelper.Serialize(requestBody);
        }

        private static string BuildGetNonceForChainAddressRequestBody(string eoaAddress)
        {
            var requestBody = new
            {
                query = "query GetNonce($input: NonceInput!) { getNonceForChainAddress(input: $input) }",
                variables = new
                {
                    input = new
                    {
                        chainAddress = eoaAddress
                    }
                }
            };
            return SerializationHelper.Serialize(requestBody);
        }

        private static string BuildSubmitTransactionRequestBody(string generatedArtm, string signedMessage)
        {
            var requestBody = new
            {
                query = "mutation SubmitTransaction($input: SubmitTransactionInput!) { submitTransaction(input: $input) { transactionHash } }",
                variables = new
                {
                    input = new
                    {
                        transaction = generatedArtm,
                        signature = signedMessage
                    }
                }
            };
            return SerializationHelper.Serialize(requestBody);
        }

        private static string BuildGetArtmStatusRequestBody(string transactionHash)
        {
            var requestBody = new
            {
                query = "query Transaction($transactionHash: TransactionHash!) { transaction(transactionHash: $transactionHash) { status error { code message } events { action args type } } }",
                variables = new
                {
                    transactionHash
                }
            };
            return SerializationHelper.Serialize(requestBody);
        }

        /// <summary>
        /// This function returns true if the response from the Futureverse Asset Register is valid
        /// </summary>
        /// <param name="response">WebResponse object</param>
        /// <param name="jToken">Response JObject</param>
        /// <returns></returns>
        private static bool IsArResponseValid(WebResponse response, out JObject jToken)
        {
            jToken = default;
            // A success code between 200 and 299 is always a success code. Only serialized Objects are allowed as response, as any other type would imply something went wrong.
            // Though technically a response code of 204 would have no body, and therefore be successful and valid, we don't allow that since the Asset Register doesn't use 204.
            return response.Successful && response.StatusCode is >= 200 and <= 299 && (jToken = SerializationHelper.Parse(response.ResponseText) as JObject) != null;
        }

        private struct GetArtmStatusResult
        {
            public readonly bool Success;
            public readonly string Status;

            public GetArtmStatusResult(bool success, string status)
            {
                Success = success;
                Status = status;
            }
        }

        /// <summary>
        /// This function actually gets the status of a transaction by its hash, not publicly accessible, mostly a helper.
        /// </summary>
        /// <exception cref="FutureverseAssetRegisterErrorException"></exception>
        private async Task<GetArtmStatusResult> RetrieveArtmStatusAsync(string transactionHash)
        {
            {
                var body = BuildGetArtmStatusRequestBody(transactionHash);
                var response = await WebRequestService.SendAsyncWebRequest(
                    RequestMethod.Post,
                    GetArApiUrl(),
                    body,
                    timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
                
                if (!IsArResponseValid(response, out var jObject) || !ParseStatus(jObject, out var transactionStatus))
                {
                    throw BuildAssetRegisterException(response, jObject);
                }

                return new GetArtmStatusResult(true, transactionStatus);
            }
        }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        public async UniTask<ArtmStatus> GetArtmStatusAsync(string transactionHash, int initialDelay, int refetchInterval, int maxRetries)
        {
            var attempts = -1;
            while (attempts < maxRetries)
            {
                var delay = attempts > -1 ? refetchInterval : initialDelay;
                if (delay > 0)
                {
                    await UniTask.Delay(delay);
                }

                var artmStatus = await RetrieveArtmStatusAsync(transactionHash);
                if (artmStatus.Success && artmStatus.Status != "PENDING")
                {
                    switch (artmStatus.Status)
                    {
                        case "PENDING":
                            return ArtmStatus.Pending;
                        case "SUCCESS":
                            return ArtmStatus.Success;
                        case "FAILED":
                        case "FAILURE": // Futureverse stated this would be the failure state, but actually it's "FAILED" so I'm covering both
                            return ArtmStatus.Failed;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(artmStatus) + "." + nameof(artmStatus.Status), "Unexpected ARTM status: " + artmStatus.Status);
                    }
                }
                
                attempts++;
            }

            return ArtmStatus.Pending;
        }

        public async UniTask<ArtmTransactionResponse> SendArtmAsync(string message, List<ArtmOperation> artmOperations, bool retrieveStatus)
        {
            if (!walletService.IsValidWallet)
            {
                throw new InvalidWalletException();
            }

            string generatedArtm;
            string signature;
            {
                // In this scope we request a nonce for the wallet, then ask the wallet to sign the generated ARTM transaction string
                var address = walletService.ChecksummedWalletAddress;
                var body = BuildGetNonceForChainAddressRequestBody(address);
                var nonceResponse = await WebRequestService.SendAsyncWebRequest(
                    RequestMethod.Post,
                    GetArApiUrl(),
                    body,
                    timeout: FutureverseSingleton.Instance.RequestTimeout * 1000
                    );

                if (!IsArResponseValid(nonceResponse, out var jObject) || !ParseNonce(jObject, out var nonce))
                {
                    throw BuildAssetRegisterException(nonceResponse, jObject);
                }

                // Nonce is valid, request to sign
                generatedArtm = ArtmBuilder.GenerateArtm(message, artmOperations, address, nonce);
                if(!EmergenceSingleton.Instance.IsCustodialLogin)
                {
                    var signatureResponse = await walletService.RequestToSignAsync(generatedArtm);
                    if (!signatureResponse.Successful)
                    {
                        throw new SignMessageFailedException(signatureResponse.Result1);
                    }

                    signature = signatureResponse.Result1;
                }
                else
                {
                    var custodialSigningService = EmergenceServiceProvider.GetService<ICustodialSigningService>();
                    signature = await custodialSigningService.RequestToSignAsync(walletService.WalletAddress,generatedArtm);
                }
            }

            string transactionHash;
            {
                // In this scope we send the generated ARTM as well as the signature so that it can be verified as a valid transaction and queued
                var body = BuildSubmitTransactionRequestBody(generatedArtm, signature);
                await UniTask.SwitchToMainThread();
                var submitResponse = await WebRequestService.SendAsyncWebRequest(
                    RequestMethod.Post,
                    GetArApiUrl(),
                    body,
                    timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
                
                if (!IsArResponseValid(submitResponse, out var jObject) || !ParseTransactionHash(jObject, out transactionHash))
                {
                    throw BuildAssetRegisterException(submitResponse, jObject);
                }
            }

            EmergenceLogger.LogInfo("Transaction Hash: " + transactionHash);

            // If retrieveStatus is true, we return a transaction response that attempts retrieving the transaction status.
            // This can be unnecessary as it would seem like a single long transaction, whilst requesting the ARTM status later
            // on will allow the developer to update the UI more often.
            return retrieveStatus
                ? new ArtmTransactionResponse(await ((IFutureverseService)this).GetArtmStatusAsync(transactionHash, maxRetries: 5), transactionHash)
                : new ArtmTransactionResponse(transactionHash);
        }
        
        private bool ParseNonce(JObject jObject, out int nonce)
        {
            var tempNonce = (int?)jObject.SelectToken("data.getNonceForChainAddress");
            if (tempNonce != null)
            {
                nonce = (int)tempNonce;
                return true;
            }

            nonce = default;
            return false;
        }
            
        private bool ParseTransactionHash(JObject jObject, out string hash)
        {
            return (hash = (string)jObject.SelectToken("data.submitTransaction.transactionHash")) != null && hash.Trim() != "";
        }
        
        private bool ParseStatus(JObject jObject, out string status)
        {
            JToken statusToken = jObject.SelectToken("data.transaction.status");
            status = statusToken?.Value<string>();
            return status != null;
        }

        /// <summary>
        /// This function will get the array of errors from the bad response, print it out and then return it
        /// </summary>
        /// <param name="responseObject">Invalid JObject provided by the Asset Register</param>
        /// <returns></returns>
        private static JArray GetAndLogArResponseErrors(JObject responseObject)
        {
            if (responseObject?["errors"] is not JArray errors) return null;
            
            foreach (var error in errors)
            {
                EmergenceLogger.LogError((string)error["message"]);
            }

            return errors;
        }

        /// <summary>
        /// This function builds an exception based on the invalid JObject provided by the Asset Register
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseObject"></param>
        /// <returns></returns>
        private static FutureverseAssetRegisterErrorException BuildAssetRegisterException(WebResponse response, JObject responseObject)
        {
            var errors = GetAndLogArResponseErrors(responseObject);
            return new FutureverseAssetRegisterErrorException(response.ResponseText, errors);
        }

        public void HandleDisconnection(ISessionService sessionService)
        {
            CurrentFuturepassInformation = null;
        }

        public void HandleConnection(ISessionService sessionService) { }
        
    }
}