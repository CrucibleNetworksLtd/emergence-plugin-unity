using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Inventory;

namespace EmergenceSDK.Integrations.Futureverse.Internal
{
    internal class FutureverseInventoryService : IInventoryService
    {
        private readonly IFutureverseService futureverseService;
        private List<string> CombinedAddress => futureverseService.FuturepassInformation.GetCombinedAddresses();

        public FutureverseInventoryService(IFutureverseService futureverseService)
        {
            this.futureverseService = futureverseService;
        }

        public async UniTask<ServiceResponse<List<InventoryItem>>> InventoryByOwnerAsync(string address, InventoryChain chain)
        {
            var futureverseInventory = await GetFutureverseInventory();
            if (!futureverseInventory.Successful)
                return new ServiceResponse<List<InventoryItem>>(futureverseInventory, false);
            var ret = new List<InventoryItem>();
            foreach (var edge in futureverseInventory.Result1.data.assets.edges)
            {
                ret.Add(ConvertFutureverseItemToInventoryItem(edge.node));
            }

            return new ServiceResponse<List<InventoryItem>>(futureverseInventory, true, ret);
        }

        public async UniTask InventoryByOwner(string address, InventoryChain chain, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {
            var response = await InventoryByOwnerAsync(address, chain);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in InventoryByOwner.", (long)response.Code);
        }

        private async UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory()
        {
            var body = SerializationHelper.Serialize(new InventoryQuery(CombinedAddress));
            var response = await WebRequestService.SendAsyncWebRequest(
                RequestMethod.Post,
                futureverseService.GetArApiUrl(),
                body,
                timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
            
            if (!response.Successful)
                return new ServiceResponse<InventoryResponse>(response, false, new InventoryResponse());

            InventoryResponse fpResponse = SerializationHelper.Deserialize<InventoryResponse>(response.ResponseText);
            return new ServiceResponse<InventoryResponse>(response, true, fpResponse);
        }

        private static InventoryItem ConvertFutureverseItemToInventoryItem(InventoryResponse.Data.Assets.Edge.Node node)
        {
            var newItem = new InventoryItem
            {
                ID = $"{node.collectionId}:{node.tokenId}",
                Blockchain = $"{node.collection.chainType}:{node.collection.chainId}",
                Contract = $"{node.collection.location}",
                TokenId = $"{node.tokenId}",
                Meta = new InventoryItemMetaData
                {
                    Name = $"#{node.tokenId}",
                    Description = node.collection.name
                }
            };
            var newMetaContent = new InventoryItemMetaContent
            {
                URL = Helpers.InternalIPFSURLToHTTP(node.metadata?.properties?.image ?? "", "http://ipfs.openmeta.xyz/ipfs/"),
                MimeType = node.metadata?.properties?.models?["glb"] != null ? "model/gltf-binary" : "image/png"
            };
            newItem.Meta.Content = new List<InventoryItemMetaContent> { newMetaContent };
            newItem.Meta.Attributes = new List<InventoryItemMetaAttributes>();
                
            if (node.metadata?.attributes != null)
            {
                foreach (var kvp in node.metadata.attributes)
                {
                    var inventoryItemMetaAttributes = new InventoryItemMetaAttributes
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    };
                    newItem.Meta.Attributes.Add(inventoryItemMetaAttributes);
                }
            }
                
            newItem.OriginalData = node.OriginalData;
            return newItem;
        }
    }
}