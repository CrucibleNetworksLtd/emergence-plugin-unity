using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    //COMING SOON: Local EVM support
    internal class LocalSessionService : SessionService
    {
        public LocalSessionService(IPersonaService personaService) : base(personaService)
        {
        }

        //Local EVM only
        public async UniTask Finish(SuccessFinish success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "finish";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.Message, e.HResult);
            }
            EmergenceUtils.PrintRequestResult("Finish request completed", request);

            if (EmergenceUtils.RequestError(request))
            {
                errorCallback?.Invoke(request.error, request.responseCode);
            }
            else
            {
                success?.Invoke();
            }
        }
    }
}