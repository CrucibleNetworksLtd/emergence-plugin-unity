using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;

namespace EmergenceSDK.Services
{
    public interface IFutureverseService : IEmergenceService
    {
        UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassInformation();

        UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturePassInformation(string futurepass);
    }
}