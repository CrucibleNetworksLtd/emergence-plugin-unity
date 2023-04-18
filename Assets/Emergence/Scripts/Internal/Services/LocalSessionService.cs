using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    internal class LocalSessionService : SessionService
    {
        public LocalSessionService(IPersonaService personaService) : base(personaService)
        {
        }

        //Local EVM only
        public async UniTask Finish(SuccessFinish success, ErrorCallback errorCallback)
        {
            string url = EmergenceSingleton.Instance.Configuration.APIBase + "finish";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest().ToUniTask();
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