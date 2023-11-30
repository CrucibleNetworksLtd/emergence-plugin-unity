using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class FutureverseService : IFutureverseService
    {
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
            
            return new ServiceResponse<FuturepassInformationResponse>(true, fpResponse);
        }
    }
}