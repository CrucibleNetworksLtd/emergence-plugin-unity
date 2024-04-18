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
        bool UsingFutureverse { get; }
        UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassInformation();
        UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturePassInformation(string futurepass);
        UniTask<List<FutureverseAssetTreePath>> GetAssetTreeAsync(string tokenId, string collectionId);
        UniTask<ArtmStatus?> SendArtmAsync(string message, string eoaAddress,
            List<FutureverseArtmOperation> artmOperations);
        UniTask<ArtmStatus> GetArtmStatus(string transactionHash, int initialDelay = 1000, int refetchInterval = 5000, int maxAttempts = 3);
    }
}