using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Exceptions;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Exceptions;
using EmergenceSDK.Types.Responses;
using Newtonsoft.Json.Linq;

namespace EmergenceSDK.Integrations.Futureverse.Internal
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
            return FutureverseSingleton.Instance.Environment switch
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
            return "https://account-indexer.api.futurepass.futureverse.app/api/v1/";
#else
            return FutureverseSingleton.Instance.Environment switch
            {
                EmergenceEnvironment.Production => "https://4yzj264is3.execute-api.us-west-2.amazonaws.com/api/v1/",
                EmergenceEnvironment.Development => "https://y4heevnpik.execute-api.us-west-2.amazonaws.com/api/v1/",
                EmergenceEnvironment.Staging => "https://y4heevnpik.execute-api.us-west-2.amazonaws.com/api/v1/",
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
            
            var url =
                $"{GetFuturepassApiUrl()}linked-futurepass?eoa={EmergenceSingleton.Instance.Configuration.Chain.ChainID}:EVM:{walletService.WalletAddress}";

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

        [Obsolete]
        private List<AssetTreePathLegacy> ParseGetAssetTreeResponseLegacy(WebResponse response)
        {
            if (response.StatusCode == 204) return new List<AssetTreePathLegacy>();
            if (!response.Successful) throw new FutureverseRequestFailedException(response);
            
            return response.StatusCode is >= 200 and <= 299
                ? ParseGetAssetTreeResponseJsonLegacy(response.ResponseText)
                : new List<AssetTreePathLegacy>();
        }

        [Obsolete]
        public List<AssetTreePathLegacy> ParseGetAssetTreeResponseJsonLegacy(string json)
        {
            var parsed = SerializationHelper.Parse(json);
            List<AssetTreePathLegacy> assetTree = new();
            if (parsed is JObject obj)
            {
                if (obj.SelectToken("data.asset.assetTree.data.@graph") is JArray dataArray)
                {
                    foreach (var data in dataArray)
                    {
                        var id = (string)data.SelectToken("@id");
                        var rdfType = (string)data.SelectToken("rdf:type.@id");
                        AssetTreePathLegacy assetPathLegacy = new(
                            id,
                            rdfType,
                            new Dictionary<string, AssetTreeObjectLegacy>()
                        );

                        if (data is JObject dataObject)
                        {
                            foreach (var property in dataObject.Properties())
                            {
                                if (property.Name is "@id" or "rdf:type") continue;

                                if (property.Value is not JObject jObject) continue;
                                var treeObject = new AssetTreeObjectLegacy(
                                    (string)jObject.SelectToken("@id"),
                                    new Dictionary<string, JToken>()
                                );

                                foreach (var childProperty in jObject.Properties())
                                {
                                    if (childProperty.Name == "@id") continue;
                                    treeObject.AdditionalData.Add(childProperty.Name, childProperty.Value);
                                }

                                assetPathLegacy.Objects.Add(property.Name, treeObject);
                            }
                        }

                        assetTree.Add(assetPathLegacy);
                    }

                    return assetTree;
                }
            }

            throw new FutureverseInvalidJsonStructureException();
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
                throw BuildAssetRegistryException(response, jObject);
            }
            
            return DeserializeGetAssetTreeResponseJson(response.ResponseText);
        }

        public List<AssetTreePath> DeserializeGetAssetTreeResponseJson(string json)
        {
            return SerializationHelper.Deserialize<List<AssetTreePath>>(SerializationHelper.Parse(json).SelectToken("data.asset.assetTree.data.@graph"));
        }

        [Obsolete]
        public async UniTask<List<AssetTreePathLegacy>> GetAssetTreeAsyncLegacy(string tokenId, string collectionId)
        {
            var body = BuildGetAssetTreeRequestBody(tokenId, collectionId);
            var response = await WebRequestService.SendAsyncWebRequest(
                RequestMethod.Post,
                GetArApiUrl(),
                body,
                timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
            
            if (!IsArResponseValid(response, out var jObject))
            {
                throw BuildAssetRegistryException(response, jObject);
            }
            
            return ParseGetAssetTreeResponseLegacy(response);
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

        private static bool IsArResponseValid(WebResponse response, out JObject jToken)
        {
            jToken = default;
            return response.Successful && response.StatusCode is >= 200 and <= 299 && (jToken = SerializationHelper.Parse(response.ResponseText) as JObject) != null;
        }

        struct GetArtmStatusResult
        {
            public readonly bool Success;
            public readonly string Status;

            public GetArtmStatusResult(bool success, string status)
            {
                Success = success;
                Status = status;
            }
        }

        /// <exception cref="FutureverseAssetRegisterErrorException"></exception>
        async Task<GetArtmStatusResult> RetrieveArtmStatusAsync(string transactionHash)
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
                    throw BuildAssetRegistryException(response, jObject);
                }

                return new(true, transactionStatus);
            }
        }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FutureverseAssetRegisterErrorException">Thrown if the Futureverse AssetRegister responds with an unexpected response</exception>
        public async UniTask<ArtmStatus> GetArtmStatusAsync(string transactionHash, int initialDelay, int refetchInterval, int maxRetries)
        {
            int attempts = -1;
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

        public async UniTask<ServiceResponse<InventoryResponse>> GetFuturepassInventoryByCollectionAndOwnerAsync(List<string> addresses, List<string> collectionIds)
        {
            var queryObject = new 
            {
                query = @"query Asset($addresses: [ChainAddress!]!, $first: Float, $collectionIds: [CollectionId!]) {
                    assets(addresses: $addresses, first: $first, collectionIds: $collectionIds) {
                        edges {
                            node {
                                metadata {
                                    properties
                                    attributes
                                }
                                collection {
                                    chainId
                                    chainType
                                    location
                                    name
                                }
                                tokenId
                                collectionId
                            }
                        }
                    }
                }",
                variables = new 
                {
                    addresses,
                    first = 1000,
                    collectionIds
                }
            };
            
            var body = SerializationHelper.Serialize(queryObject);
            var response = await WebRequestService.SendAsyncWebRequest(
                RequestMethod.Post,
                GetArApiUrl(),
                body,
                timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
            
            if (!response.Successful)
                return new ServiceResponse<InventoryResponse>(response, false, new InventoryResponse());

            var fpResponse = SerializationHelper.Deserialize<InventoryResponse>(response.ResponseText);
            return new ServiceResponse<InventoryResponse>(response, true, fpResponse);
        }

        public async Task<ArtmTransactionResponse> SendArtmAsync(string message, List<ArtmOperation> artmOperations, bool retrieveStatus)
        {
            if (!walletService.IsValidWallet)
            {
                throw new InvalidWalletException();
            }

            string generatedArtm;
            string signature;
            
            {
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
                    throw BuildAssetRegistryException(nonceResponse, jObject);
                }

                generatedArtm = ArtmBuilder.GenerateArtm(message, artmOperations, address, nonce);
                var signatureResponse = await walletService.RequestToSignAsync(generatedArtm);
                if (!signatureResponse.Successful)
                {
                    throw new SignMessageFailedException(signatureResponse.Result1);
                }

                signature = signatureResponse.Result1;
            }

            string transactionHash;
            {
                var body = BuildSubmitTransactionRequestBody(generatedArtm, signature);
                var submitResponse = await WebRequestService.SendAsyncWebRequest(
                    RequestMethod.Post,
                    GetArApiUrl(),
                    body,
                    timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
                
                if (!IsArResponseValid(submitResponse, out var jObject) || !ParseTransactionHash(jObject, out transactionHash))
                {
                    throw BuildAssetRegistryException(submitResponse, jObject);
                }
            }

            EmergenceLogger.LogInfo("Transaction Hash: " + transactionHash);

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

        private static JArray GetAndLogArResponseErrors(JObject responseObject)
        {
            if (responseObject?["errors"] is not JArray errors) return null;
            
            foreach (var error in errors)
            {
                EmergenceLogger.LogError((string)error["message"]);
            }

            return errors;
        }

        private static FutureverseAssetRegisterErrorException BuildAssetRegistryException(WebResponse response, JObject responseObject)
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