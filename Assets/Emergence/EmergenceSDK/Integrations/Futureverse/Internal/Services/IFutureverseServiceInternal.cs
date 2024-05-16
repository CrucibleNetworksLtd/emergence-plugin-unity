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
        FuturepassInformationResponse FuturepassInformation { set; }
        UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory();
        UniTask<ServiceResponse<List<InventoryItem>>> GetFutureverseInventoryAsInventoryItems();
        [Obsolete] List<AssetTreePathLegacy> ParseGetAssetTreeResponseJsonLegacy(string json);
        List<AssetTreePath> DeserializeGetAssetTreeResponseJson(string json);
    }
}