using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Futureverse.Types;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Inventory;
using EmergenceSDK.Types.Responses;
using EmergenceSDK.Types.Responses.FuturePass;

namespace EmergenceSDK.Futureverse.Services
{
    public interface IFutureverseService : IEmergenceService
    {
        bool UsingFutureverse { get; }
        UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassInformation();

        UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturePassInformation(string futurepass);

        UniTask<ServiceResponse<FVInventoryResponse>> GetFutureverseInventory();
        UniTask<ServiceResponse<List<InventoryItem>>> GetFutureverseInventoryAsInventoryItems();
    }
}