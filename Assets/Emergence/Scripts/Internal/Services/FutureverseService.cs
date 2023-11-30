using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using EmergenceSDK.Types.Responses.FuturePass;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class FutureverseService : IFutureverseService
    {
        public bool UsingFutureverse {get; private set;} = false;
        
        private FuturepassInformationResponse FuturepassInformation { get; set; }
        
        private List<string> combinedAddress => FuturepassInformation.GetCombinedAddresses();
        
        private const string fpassURL = "https://4yzj264is3.execute-api.us-west-2.amazonaws.com/api/v1/";
        public async UniTask<ServiceResponse<LinkedFuturepassResponse>> GetLinkedFuturepassInformation()
        {
            var url = $"{fpassURL}linked-futurepass?eoa=1:EVM:{EmergenceServices.GetService<IWalletService>().WalletAddress}";
            
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<LinkedFuturepassResponse>(false);

            LinkedFuturepassResponse fpResponse = SerializationHelper.Deserialize<LinkedFuturepassResponse>(response.Response);
            
            return new ServiceResponse<LinkedFuturepassResponse>(true, fpResponse);
        }
        
        public async UniTask<ServiceResponse<FuturepassInformationResponse>> GetFuturePassInformation(string futurepass)
        {
            var url = $"{fpassURL}linked-eoa?futurepass={futurepass}";
            
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbGET, url, EmergenceLogger.LogError);
            if(response.IsSuccess == false)
                return new ServiceResponse<FuturepassInformationResponse>(false);
            
            FuturepassInformationResponse fpResponse = SerializationHelper.Deserialize<FuturepassInformationResponse>(response.Response);
            UsingFutureverse = true;
            FuturepassInformation = fpResponse;
            return new ServiceResponse<FuturepassInformationResponse>(true, fpResponse);
        }
        
        public async UniTask<ServiceResponse<FVInventoryResponse>> GetFutureverseInventory()
        {
            var url = EmergenceSingleton.Instance.Environment == Environment.Production ?
                        "https://w1jv6xw3jh.execute-api.us-west-2.amazonaws.com/api/graphql":
                        "https://adx1wewtnh.execute-api.us-west-2.amazonaws.com/api/graphql";
            var query = SerializationHelper.Serialize(new FVInventoryQuery(combinedAddress));
            var response = await WebRequestService.PerformAsyncWebRequest(UnityWebRequest.kHttpVerbPOST, url, EmergenceLogger.LogError, query);
            if(response.IsSuccess == false)
                return new ServiceResponse<FVInventoryResponse>(false, new FVInventoryResponse());
            
            FVInventoryResponse fpResponse = SerializationHelper.Deserialize<FVInventoryResponse>(response.Response);
            return new ServiceResponse<FVInventoryResponse>(true, fpResponse);
        }
    }
}