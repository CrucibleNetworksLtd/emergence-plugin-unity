using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
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
            if (FutureverseSingleton.Instance.selectedEnvironment == FutureverseSingleton.Environment.Production)
            {
                return "https://ar-api.futureverse.app/graphql";
            }

            if (FutureverseSingleton.Instance.selectedEnvironment == FutureverseSingleton.Environment.Development)
            {
                return "https://ar-api.futureverse.dev/graphql";
            }

            if (FutureverseSingleton.Instance.selectedEnvironment == FutureverseSingleton.Environment.Staging)
            {
                return "https://ar-api.futureverse.cloud/graphql";
            }

            throw new ArgumentOutOfRangeException();
#endif
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
            var url = FutureverseSingleton.Instance.selectedEnvironment == FutureverseSingleton.Environment.Production
                ? "https://w1jv6xw3jh.execute-api.us-west-2.amazonaws.com/api/graphql"
                : "https://adx1wewtnh.execute-api.us-west-2.amazonaws.com/api/graphql";
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
            foreach (var edge in futureverseInventory.Result.data.nfts.edges)
            {
                var node = edge.node;
                var newItem = new InventoryItem();
                newItem.ID =
                    $"{node.collection.chainType}:{node.collection.chainId}:{node.collection.location}:{node.tokenIdNumber}";
                newItem.Blockchain = $"{node.collection.chainType}:{node.collection.chainId}";
                newItem.Contract = $"{node.collection.location}";
                newItem.TokenId = $"{node.tokenIdNumber}";
                newItem.Meta = new InventoryItemMetaData();
                newItem.Meta.Name = $"#{node.tokenIdNumber}";
                newItem.Meta.Description = node.collection.name;
                var newMetaContent = new InventoryItemMetaContent();
                newMetaContent.URL = node.metadata.properties.image;
                newMetaContent.MimeType = node.metadata.properties.glb_url == null ? "model/gltf-binary" : "image/png";
                newItem.Meta.Content = new List<InventoryItemMetaContent>();
                newItem.Meta.Content.Add(newMetaContent);

                ret.Add(newItem);
            }

            return new ServiceResponse<List<InventoryItem>>(true, ret);
        }

        private static List<FutureverseAssetTreePath> ParseGetAssetTreeResponse(WebResponse response)
        {
            List<FutureverseAssetTreePath> assetTree = new();
            if (response.IsSuccess)
            {
                var statusCode = response.StatusCode;
                if (statusCode is >= 200 and <= 299)
                {
                    JToken parsed = SerializationHelper.Parse(response.Response);
                    if (parsed is JObject)
                    {
                        var obj = (JObject)parsed;
                        var dataArray = (JArray)obj["data"]?["asset"]?["assetTree"]?["data"]?["@graph"];
                        if (dataArray != null)
                        {
                            foreach (var data in dataArray)
                            {
                                FutureverseAssetTreePath assetPath = new(
                                    (string)data["@id"],
                                    (string)data["rdf:type"]?["@id"],
                                    new()
                                );

                                foreach (var property in ((JObject)data).Properties())
                                {
                                    if (property.Name is "@id" or "rdf:type")
                                    {
                                        //ignore these ones
                                        continue;
                                    }

                                    var treeObject = new FutureverseAssetTreeObject((string)data["@id"], new());
                                    if (property.Value is JObject jObject)
                                    {
                                        foreach (var childProperty in jObject.Properties())
                                        {
                                            if (childProperty.Name == "@id")
                                            {
                                                //ignore
                                                continue;
                                            }

                                            treeObject.AdditionalData.Add(childProperty.Name, childProperty);
                                        }
                                    }

                                    assetPath.Objects.Add(property.Name, treeObject);
                                }

                                assetTree.Add(assetPath);
                            }

                            return assetTree;
                        }
                    }
                }
            }
            
            return assetTree;
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
    }
}