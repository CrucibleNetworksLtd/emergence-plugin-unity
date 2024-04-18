using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Integrations.Futureverse.Types.Responses;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Integrations.Futureverse.Services
{
    public interface IFutureverseService : IEmergenceService
    {
        FutureverseSingleton.Environment GetEnvironment();

        void RunInForcedEnvironment(FutureverseSingleton.Environment environment, Action action);
        UniTask RunInForcedEnvironmentAsync(FutureverseSingleton.Environment environment, Func<UniTask> action);
        
        bool UsingFutureverse { get; }
        UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassInformation();

        UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturePassInformation(string futurepass);

        UniTask<ServiceResponse<InventoryResponse>> GetFutureverseInventory();
        UniTask<ServiceResponse<List<InventoryItem>>> GetFutureverseInventoryAsInventoryItems();

        List<FutureverseAssetTreePath> ParseGetAssetTreeJson(string json);
        UniTask<List<FutureverseAssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId);
    }
}