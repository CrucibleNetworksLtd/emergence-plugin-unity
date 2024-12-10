using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Futureverse.Types;
using EmergenceSDK.Runtime.Futureverse.Types.Responses;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Inventory;
using Newtonsoft.Json;
using UnityEngine;

namespace EmergenceSDK.Runtime.Futureverse.Internal
{
    internal class FutureverseInventoryService : IFutureverseInventoryService
    {
        private readonly IFutureverseService futureverseService;
        private List<string> CombinedAddress => futureverseService.CurrentFuturepassInformation.GetCombinedAddresses();

        public FutureverseInventoryService(IFutureverseService futureverseService)
        {
            this.futureverseService = futureverseService;
        }
        
                
        public async UniTask InventoryByOwner(string address, InventoryChain chain, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {
            var response = await InventoryByOwnerAsync(address, chain);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in InventoryByOwner.", (long)response.Code);
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
        
        public async UniTask InventoryByOwnerAndCollection(List<string> collectionIds, SuccessInventoryByOwner success, ErrorCallback errorCallback)
        {
            var response = await InventoryByOwnerAndCollectionAsync(collectionIds);
            if (response.Successful)
            {
                success?.Invoke(response.Result1);
            }
            else
            {
                errorCallback?.Invoke("Error in InventoryByOwnerAndCollection.", (long)response.Code);
            }
        }

        public async UniTask<ServiceResponse<List<InventoryItem>>> InventoryByOwnerAndCollectionAsync(List<string> collectionIds)
        {
            var futureverseInventoryResponse = await GetFutureverseInventory();

            if (!futureverseInventoryResponse.Successful)
            {
                return new ServiceResponse<List<InventoryItem>>(futureverseInventoryResponse, false, new List<InventoryItem>());
            }

            var inventoryItems = new List<InventoryItem>();

            foreach (var edge in futureverseInventoryResponse.Result1.data.assets.edges)
            {
                if (edge.node.collectionId != null && collectionIds.Contains(edge.node.collectionId))
                {
                    inventoryItems.Add(ConvertFutureverseItemToInventoryItem(edge.node));
                }
            }

            return new ServiceResponse<List<InventoryItem>>(futureverseInventoryResponse, true, inventoryItems);
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

            var fpResponse = SerializationHelper.Deserialize<InventoryResponse>(response.ResponseText);
            return new ServiceResponse<InventoryResponse>(response, true, fpResponse);
        }
        
        /// <summary>
        /// Returns an inventory deserialised into type T, where T is a user provided record format.
        /// An alternate to the SDK provided inventory record, allows users to dictate data consumption.
        /// </summary>
        /// <param name="addressList">Can be used to provide an override list of addresses</param>
        /// <typeparam name="T">Record for data serialisation</typeparam>
        /// <returns>Returns a service response of type T for easier interrogation.</returns>
        public async UniTask<ServiceResponse<T>> GetInventoryAs<T>(List<string> addressList = null) where T : class, new()
        {
            var currentAddressList = addressList ?? CombinedAddress;
            
            var body = SerializationHelper.Serialize(new InventoryQuery(currentAddressList));
            var response = await WebRequestService.SendAsyncWebRequest(
                RequestMethod.Post,
                futureverseService.GetArApiUrl(),
                body,
                timeout: FutureverseSingleton.Instance.RequestTimeout * 1000);
    
            if (!response.Successful)
                return new ServiceResponse<T>(response, false, new T());
            
            try
            {
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore, // Ignore extra JSON fields
                    NullValueHandling = NullValueHandling.Include,        // Include null values
                };

                var deserializedResponse = JsonConvert.DeserializeObject<T>(response.ResponseText, settings);
                return new ServiceResponse<T>(response, true, deserializedResponse);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialise response into type T, validate structure and try again");
            }
            return new ServiceResponse<T>(response, false, new T());
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