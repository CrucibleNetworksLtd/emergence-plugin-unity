using System;
using System.Collections.Generic;
using System.Text;
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
using EmergenceSDK.Types.Inventory;
using EmergenceSDK.Types.Responses;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace EmergenceSDK.Integrations.Futureverse.Internal
{
    internal class FutureverseService : IFutureverseService, IFutureverseServiceInternal, IDisconnectableService
    {
        public bool UsingFutureverse { get; set; }
        public FuturepassInformationResponse FuturepassInformation { get; set; }

        private List<string> CombinedAddress => FuturepassInformation.GetCombinedAddresses();
        
        private string GetArApiUrl()
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
        
        private string GetFuturepassApiUrl()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            return "https://account-indexer.api.futurepass.futureverse.app/api/v1/";
#else
            return EmergenceSingleton.Instance.Environment switch
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
            var walletService = EmergenceServiceProvider.GetService<IWalletService>();
            if (!walletService.IsValidWallet)
            {
                throw new InvalidWalletException();
            }
            
            var url =
                $"{GetFuturepassApiUrl()}linked-futurepass?eoa={EmergenceSingleton.Instance.Configuration.Chain.ChainID}:EVM:{walletService.WalletAddress}";

            var response =
                await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url,
                    EmergenceLogger.LogError);
            if (response.IsSuccess == false)
                return new ServiceResponse<LinkedFuturepassResponse>(false);

            LinkedFuturepassResponse fpResponse =
                SerializationHelper.Deserialize<LinkedFuturepassResponse>(response.Response);

            return new ServiceResponse<LinkedFuturepassResponse>(true, fpResponse);
        }

        public async UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturepassInformationAsync(string futurepass)
        {
            var url = $"{GetFuturepassApiUrl()}linked-eoa?futurepass={futurepass}";

            var response =
                await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url,
                    EmergenceLogger.LogError);
            if (!response.IsSuccess)
                return new ServiceResponse<FuturepassInformationResponse>(false);

            FuturepassInformationResponse fpResponse =
                SerializationHelper.Deserialize<FuturepassInformationResponse>(response.Response);
            return new ServiceResponse<FuturepassInformationResponse>(true, fpResponse);
        }

        public async UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory()
        {
            var url = GetArApiUrl();
            var query = SerializationHelper.Serialize(new InventoryQuery(CombinedAddress));
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url,
                EmergenceLogger.LogError, query);
            if (!response.IsSuccess)
                return new ServiceResponse<InventoryResponse>(false, new InventoryResponse());

            InventoryResponse fpResponse = SerializationHelper.Deserialize<InventoryResponse>(response.Response);
            return new ServiceResponse<InventoryResponse>(true, fpResponse);
        }

        public async UniTask<ServiceResponse<List<InventoryItem>>> GetFutureverseInventoryAsInventoryItems()
        {
            var futureverseInventory = await GetFutureverseInventory();
            if (futureverseInventory.Success == false)
                return new ServiceResponse<List<InventoryItem>>(false);
            var ret = new List<InventoryItem>();
            foreach (var edge in futureverseInventory.Result.data.assets.edges)
            {
                var node = edge.node;
                var newItem = new InventoryItem();
                newItem.ID =
                    $"{node.collection.chainType}:{node.collection.chainId}:{node.collection.location}:{node.tokenId}";
                newItem.Blockchain = $"{node.collection.chainType}:{node.collection.chainId}";
                newItem.Contract = $"{node.collection.location}";
                newItem.TokenId = $"{node.tokenId}";
                newItem.Meta = new InventoryItemMetaData
                {
                    Name = $"#{node.tokenId}",
                    Description = node.collection.name
                };
                var newMetaContent = new InventoryItemMetaContent
                {
                    URL = Helpers.InternalIPFSURLToHTTP(node.metadata?.properties?.image ?? "", "http://ipfs.openmeta.xyz/ipfs/"),
                    MimeType = node.metadata?.properties?.models?["glb"] != null ? "model/gltf-binary" : "image/png"
                };
                newItem.Meta.Content = new List<InventoryItemMetaContent> { newMetaContent };
                newItem.Meta.Attributes = new List<InventoryItemMetaAttributes>();
                foreach (var kvp in node.metadata?.attributes ?? new Dictionary<string, string>())
                {
                    var inventoryItemMetaAttributes = new InventoryItemMetaAttributes
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    };
                    newItem.Meta.Attributes.Add(inventoryItemMetaAttributes);
                }
                newItem.OriginalData = node.OriginalData;

                ret.Add(newItem);
            }

            return new ServiceResponse<List<InventoryItem>>(true, ret);
        }

        private List<FutureverseAssetTreePath> ParseGetAssetTreeResponse(WebResponse response)
        {
            if (response.StatusCode == 204) return new List<FutureverseAssetTreePath>();
            if (!response.IsSuccess) throw new FutureverseRequestFailedException(response);
            
            return response.StatusCode is >= 200 and <= 299
                ? ParseGetAssetTreeJson(response.Response)
                : new List<FutureverseAssetTreePath>();
        }

        public List<FutureverseAssetTreePath> ParseGetAssetTreeJson(string json)
        {
            var parsed = SerializationHelper.Parse(json);
            List<FutureverseAssetTreePath> assetTree = new();
            if (parsed is JObject obj)
            {
                if (obj.SelectToken("data.asset.assetTree.data.@graph") is JArray dataArray)
                {
                    foreach (var data in dataArray)
                    {
                        var id = (string)data.SelectToken("@id");
                        var rdfType = (string)data.SelectToken("rdf:type.@id");
                        FutureverseAssetTreePath assetPath = new(
                            id,
                            rdfType,
                            new Dictionary<string, FutureverseAssetTreeObject>()
                        );

                        if (data is JObject dataObject)
                        {
                            foreach (var property in dataObject.Properties())
                            {
                                if (property.Name is "@id" or "rdf:type") continue;

                                if (property.Value is not JObject jObject) continue;
                                var treeObject = new FutureverseAssetTreeObject(
                                    (string)jObject.SelectToken("@id"),
                                    new Dictionary<string, JToken>()
                                );

                                foreach (var childProperty in jObject.Properties())
                                {
                                    if (childProperty.Name == "@id") continue;
                                    treeObject.AdditionalData.Add(childProperty.Name, childProperty.Value);
                                }

                                assetPath.Objects.Add(property.Name, treeObject);
                            }
                        }

                        assetTree.Add(assetPath);
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

        public async UniTask<List<FutureverseAssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId)
        {
            var body = BuildGetAssetTreeRequestBody(tokenId, collectionId);
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = FutureverseSingleton.Instance.requestTimeout;
            var response = await WebRequestService.PerformAsyncWebRequest(request, (message, code) => { });
            if (!IsArResponseValid(response, out var jObject))
            {
                LogArResponseErrors(jObject);
                throw new FutureverseAssetRegisterErrorException(response.Response);
            }
            
            return ParseGetAssetTreeResponse(response);
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
            return response.IsSuccess && response.StatusCode is >= 200 and <= 299 && (jToken = SerializationHelper.Parse(response.Response) as JObject) != null;
        }

        private static void LogArResponseErrors(JObject jObject)
        {
            if (jObject?["errors"] is not JArray errors) return;
            
            foreach (var error in errors)
            {
                EmergenceLogger.LogError((string)error["message"]);
            }
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
                using var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = FutureverseSingleton.Instance.requestTimeout;
                var response = await WebRequestService.PerformAsyncWebRequest(request, null);
                
                if (!IsArResponseValid(response, out var jObject) || !ParseStatus(jObject, out var transactionStatus))
                {
                    LogArResponseErrors(jObject);
                    throw new FutureverseAssetRegisterErrorException(response.Response);
                }

                return new(true, transactionStatus);
            }
            
            bool ParseStatus(JObject jObject, out string status)
            {
                status = jObject["data"]?["transaction"]?["status"]?.Value<string>();
                return status != null;
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
        
        public async Task<ArtmTransactionResponse> SendArtmAsync(string message, List<FutureverseArtmOperation> artmOperations, bool retrieveStatus)
        {
            var walletService = EmergenceServiceProvider.GetService<IWalletService>();

            if (!walletService.IsValidWallet)
            {
                throw new InvalidWalletException();
            }

            string generatedArtm;
            string signature;
            
            {
                var address = walletService.ChecksummedWalletAddress;
                var body = BuildGetNonceForChainAddressRequestBody(address);
                using var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = FutureverseSingleton.Instance.requestTimeout;
                var nonceResponse = await WebRequestService.PerformAsyncWebRequest(request, (_, _) => { });
                
                if (!IsArResponseValid(nonceResponse, out var jObject) || !ParseNonce(jObject, out var nonce))
                {
                    LogArResponseErrors(jObject);
                    throw new FutureverseAssetRegisterErrorException(nonceResponse.Response);
                }

                generatedArtm = ArtmBuilder.GenerateArtm(message, artmOperations, address, nonce);
                var signatureResponse = await walletService.RequestToSignAsync(generatedArtm);
                if (!signatureResponse.Success)
                {
                    throw new SignMessageFailedException(signatureResponse.Result);
                }

                signature = signatureResponse.Result;
            }

            string transactionHash;
            {
                var body = BuildSubmitTransactionRequestBody(generatedArtm, signature);
                using var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = FutureverseSingleton.Instance.requestTimeout;
                var submitResponse = await WebRequestService.PerformAsyncWebRequest(request, (_, _) => { });
                
                if (!IsArResponseValid(submitResponse, out var jObject) || !ParseTransactionHash(jObject, out transactionHash))
                {
                    LogArResponseErrors(jObject);
                    throw new FutureverseAssetRegisterErrorException(submitResponse.Response);
                }
            }

            EmergenceLogger.LogInfo("Transaction Hash: " + transactionHash);

            return retrieveStatus
                ? new ArtmTransactionResponse(await ((IFutureverseService)this).GetArtmStatusAsync(transactionHash, maxRetries: 5), transactionHash)
                : new ArtmTransactionResponse(transactionHash);

            bool ParseNonce(JObject jObject, out int nonce)
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
            
            bool ParseTransactionHash(JObject jObject, out string hash)
            {
                return (hash = (string)jObject.SelectToken("data.submitTransaction.transactionHash")) != null && hash.Trim() != "";
            }
        }

        public void HandleDisconnection()
        {
            UsingFutureverse = false;
            FuturepassInformation = null;
        }
    }
}