using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Exceptions;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using EmergenceSDK.Types.Responses;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace EmergenceSDK.Integrations.Futureverse.Internal
{
    internal class FutureverseService : IFutureverseService
    {
        public bool UsingFutureverse { get; private set; } = false;

        private FuturepassInformationResponse FuturepassInformation { get; set; }

        private List<string> CombinedAddress => FuturepassInformation.GetCombinedAddresses();
        
        private string GetArApiUrl()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            return "https://ar-api.futureverse.app/graphql";
#else
            return GetEnvironment() switch
            {
                FutureverseSingleton.Environment.Production => "https://ar-api.futureverse.app/graphql",
                FutureverseSingleton.Environment.Development => "https://ar-api.futureverse.dev/graphql",
                FutureverseSingleton.Environment.Staging => "https://ar-api.futureverse.cloud/graphql",
                _ => throw new ArgumentOutOfRangeException()
            };
#endif
        }

        public FutureverseSingleton.Environment GetEnvironment()
        {
            return ForcedEnvironment ?? FutureverseSingleton.Instance.selectedEnvironment;
        }

        public FutureverseSingleton.Environment? ForcedEnvironment { get; set; }

        public void RunInForcedEnvironment(FutureverseSingleton.Environment environment, Action action)
        {
            var prevForcedEnvironment = ForcedEnvironment;
            ForcedEnvironment = environment;
            try
            {
                action.Invoke();
            }
            finally
            {
                ForcedEnvironment = prevForcedEnvironment;
            }
        }

        public async UniTask RunInForcedEnvironmentAsync(FutureverseSingleton.Environment environment, Func<UniTask> action)
        {
            var prevForcedEnvironment = ForcedEnvironment;
            ForcedEnvironment = environment;
            try
            {
                await action();
            }
            finally
            {
                ForcedEnvironment = prevForcedEnvironment;
            }
        }
        
        private string GetFuturePassApiUrl()
        {
            return "https://4yzj264is3.execute-api.us-west-2.amazonaws.com/api/v1/";
        }

        public async UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassInformation()
        {
            var url =
                $"{GetFuturePassApiUrl()}linked-futurepass?eoa=1:EVM:{EmergenceServiceProvider.GetService<IWalletService>().WalletAddress}";

            var response =
                await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url,
                    EmergenceLogger.LogError);
            if (response.IsSuccess == false)
                return new ServiceResponse<LinkedFuturepassResponse>(false);

            LinkedFuturepassResponse fpResponse =
                SerializationHelper.Deserialize<LinkedFuturepassResponse>(response.Response);

            return new ServiceResponse<LinkedFuturepassResponse>(true, fpResponse);
        }

        public async UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturePassInformation(string futurepass)
        {
            var url = $"{GetFuturePassApiUrl()}linked-eoa?futurepass={futurepass}";

            var response =
                await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url,
                    EmergenceLogger.LogError);
            if (response.IsSuccess == false)
                return new ServiceResponse<FuturepassInformationResponse>(false);

            FuturepassInformationResponse fpResponse =
                SerializationHelper.Deserialize<FuturepassInformationResponse>(response.Response);
            UsingFutureverse = true;
            EmergenceServiceProvider.GetService<ISessionService>().OnSessionDisconnected += OnSessionDisconnected;
            FuturepassInformation = fpResponse;
            return new ServiceResponse<FuturepassInformationResponse>(true, fpResponse);
        }

        private void OnSessionDisconnected()
        {
            UsingFutureverse = false;
            FuturepassInformation = null;
            EmergenceServiceProvider.GetService<ISessionService>().OnSessionDisconnected -= OnSessionDisconnected;
        }

        public async UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory()
        {
            var url = GetArApiUrl();
            var query = SerializationHelper.Serialize(new InventoryQuery(CombinedAddress));
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url,
                EmergenceLogger.LogError, query);
            if (response.IsSuccess == false)
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
                newItem.Meta = new InventoryItemMetaData();
                newItem.Meta.Name = $"#{node.tokenId}";
                newItem.Meta.Description = node.collection.name;
                var newMetaContent = new InventoryItemMetaContent();
                newMetaContent.URL = node.metadata.properties.image;
                newMetaContent.MimeType = node.metadata.properties.models?["glb"] != null ? "model/gltf-binary" : "image/png";
                newItem.Meta.Content = new List<InventoryItemMetaContent>();
                newItem.Meta.Content.Add(newMetaContent);
                foreach (var kvp in node.metadata.attributes)
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
                var dataArray = (JArray)obj["data"]?["asset"]?["assetTree"]?["data"]?["@graph"];
                if (dataArray != null)
                {
                    foreach (var data in dataArray)
                    {
                        FutureverseAssetTreePath assetPath = new(
                            (string)data["@id"],
                            (string)data["rdf:type"]?["@id"],
                            new Dictionary<string, FutureverseAssetTreeObject>()
                        );

                        foreach (var property in ((JObject)data).Properties())
                        {
                            if (property.Name is "@id" or "rdf:type") continue;

                            if (property.Value is JObject jObject)
                            {
                                var treeObject = new FutureverseAssetTreeObject((string)jObject["@id"], new Dictionary<string, JToken>());
                                
                                foreach (var childProperty in jObject.Properties())
                                {
                                    if (childProperty.Name == "@id") continue;
                                    treeObject.AdditionalData.Add(childProperty.Name, childProperty);
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
            return $@"{{""query"":""query Asset($tokenId: String!, $collectionId: CollectionId!) {{ asset(tokenId: $tokenId, collectionId: $collectionId) {{ assetTree {{ data }} }} }}"",""variables"":{{""tokenId"":""{tokenId}"",""collectionId"":""{collectionId}""}}}}";
        }

        public async UniTask<List<FutureverseAssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId)
        {
            var body = BuildGetAssetTreeRequestBody(tokenId, collectionId);
            var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = FutureverseSingleton.Instance.requestTimeout;
            var response = await WebRequestService.PerformAsyncWebRequest(request, (message, code) => { });
            return ParseGetAssetTreeResponse(response);
        }
        
        private static string BuildGetNonceForChainAddressRequestBody(string eoaAddress)
        {
            return $@"{{""query"":""query GetNonce($input: NonceInput!) {{ getNonceForChainAddress(input: $input) }}"",""variables"":{{""input"":{{""chainAddress"":""{eoaAddress}""}}}}}}";        }
        
        private static string BuildSubmitTransactionRequestBody(string generatedArtm, string signedMessage)
        {
            return $@"{{""query"":""mutation SubmitTransaction($input: SubmitTransactionInput!) {{ submitTransaction(input: $input) {{ transactionHash }} }}"",""variables"":{{""input"":{{""transaction"":""{generatedArtm}"",""signature"":""{signedMessage}""}}}}}}";        }
        
        private static string BuildGetArtmStatusRequestBody(string transactionHash)
        {
            return $@"{{""query"":""query Transaction($transactionHash: TransactionHash!) {{ transaction(transactionHash: {{_TransactionHash}}) {{ status error {{ code message }} events {{ action args type }} }} }}"",""variables"":{{""transactionHash"":""{transactionHash}""}}}}";        }

        private static bool IsArResponseValid(string body, out JObject jObject)
        {
            jObject = default;
            var parsed = SerializationHelper.Parse(body);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return parsed is JObject jObj && jObj["errors"] == null && (jObject = jObj) != null; // Last part is for setting out parameter in one line, as it's always true
        }

        public static bool IsArResponseValid(WebResponse response, out JObject jObject)
        {
            jObject = default;
            return response.IsSuccess && response.StatusCode is >= 200 and <= 299 && IsArResponseValid(response.Response, out jObject);
        }

        public static void LogArResponseErrors(JObject jObject)
        {
            var errors = (JArray)jObject["errors"];
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

        async Task<GetArtmStatusResult> RetrieveArtmStatusAsync(string transactionHash)
        {
            {
                var body = BuildGetArtmStatusRequestBody(transactionHash);
                using var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = FutureverseSingleton.Instance.requestTimeout;
                var nonceResponse = await WebRequestService.PerformAsyncWebRequest(request, (errorMessage, code) => { });
                
                if (!IsArResponseValid(nonceResponse, out var jObject) || !ParseStatus(jObject, out var transactionStatus))
                {
                    LogArResponseErrors(jObject);
                    return new (false, "");
                }

                return new(true, transactionStatus);
            }
            
            bool ParseStatus(JObject jObject, out string status)
            {
                status = jObject["data"]?["transaction"]?["status"]?.Value<string>();
                return status != null;
            }
        }
        
        public async UniTask<ArtmStatus> GetArtmStatus(string transactionHash, int initialDelay = 1000, int refetchInterval = 5000, int maxAttempts = 3)
        {
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                var delay = attempts > 0 ? refetchInterval : initialDelay;
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
                        case "FAILURE":
                            return ArtmStatus.Failure;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(artmStatus.Status), "Unexpected ARTM status: " + artmStatus.Status);
                    }
                }
                
                attempts++;
            }
            
            throw new ExhaustedRequestAttemptsException();
        }
        
        public async UniTask<bool> SendArtmAsync(string message,
            string eoaAddress,
            List<FutureverseArtmOperation> artmOperations)
        {
            string generatedArtm;
            string signature;
            
            {
                var body = BuildGetNonceForChainAddressRequestBody(eoaAddress);
                using var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = FutureverseSingleton.Instance.requestTimeout;
                var nonceResponse = await WebRequestService.PerformAsyncWebRequest(request, (errorMessage, code) => { });
                
                if (!IsArResponseValid(nonceResponse, out var jObject) || !ParseNonce(jObject, out var nonce))
                {
                    LogArResponseErrors(jObject);
                    return false;
                }

                generatedArtm = ArtmBuilder.GenerateArtm(message, artmOperations, eoaAddress, nonce);
                var signatureResponse = await EmergenceServiceProvider.GetService<IWalletService>().RequestToSignAsync(generatedArtm);
                if (!signatureResponse.Success)
                {
                    return false;
                }

                signature = signatureResponse.Result;
            }

            string transactionHash;
            {
                var body = BuildSubmitTransactionRequestBody(generatedArtm, signature);
                using var request = WebRequestService.CreateRequest(UnityWebRequest.kHttpVerbPOST, GetArApiUrl(), body);
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = FutureverseSingleton.Instance.requestTimeout;
                var submitResponse = await WebRequestService.PerformAsyncWebRequest(request, (errorMessage, code) => { });
                
                if (!IsArResponseValid(submitResponse, out var jObject) || !ParseTransactionHash(jObject, out transactionHash))
                {
                    LogArResponseErrors(jObject);
                    return false;
                }
            }
            
            return await GetArtmStatus(transactionHash) != ArtmStatus.Pending;

            bool ParseNonce(JObject jObject, out int nonce)
            {
                var tempNonce = jObject["data"]?["getNonceForChainAddress"]?.Value<int>();
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
                hash = jObject["data"]?["submitTransaction"]?["transactionHash"]?.Value<string>();
                return hash != null;
            }
        }
    }
}