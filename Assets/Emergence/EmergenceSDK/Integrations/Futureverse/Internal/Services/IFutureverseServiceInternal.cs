using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
{
    internal interface IFutureverseServiceInternal : IEmergenceService
    {
        public bool UsingFutureverse { get; set; }
        UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory();
        UniTask<ServiceResponse<List<InventoryItem>>> GetFutureverseInventoryAsInventoryItems();
        List<FutureverseAssetTreePath> ParseGetAssetTreeJson(string json);
    }
}