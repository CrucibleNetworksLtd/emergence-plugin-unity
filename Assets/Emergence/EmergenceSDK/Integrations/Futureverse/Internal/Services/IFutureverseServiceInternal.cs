using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
{
    internal interface IFutureverseServiceInternal : IEmergenceService
    {
        FutureverseSingleton.Environment? ForcedEnvironment { get; set; }
        void RunInForcedEnvironment(FutureverseSingleton.Environment environment, Action action);
        UniTask RunInForcedEnvironmentAsync(FutureverseSingleton.Environment environment, Func<UniTask> action);
        
        UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory();
        UniTask<ServiceResponse<List<InventoryItem>>> GetFutureverseInventoryAsInventoryItems();
        List<FutureverseAssetTreePath> ParseGetAssetTreeJson(string json);
    }
}