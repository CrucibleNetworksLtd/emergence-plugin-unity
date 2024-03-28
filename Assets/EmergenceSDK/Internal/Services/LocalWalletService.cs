using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;
using EmergenceSDK.Types.Responses;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Services
{
    //COMING SOON: Local EVM support
    internal class LocalWalletService : WalletService
    {
        
        public LocalWalletService(IPersonaService personaService, ISessionService sessionService) : base(personaService, sessionService)
        {
        }
        
        //Local EVM only
        public async UniTask CreateWallet(string path, string password, CreateWalletSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "createWallet" + "?path=" + path +
                         "&password=" + password;

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.method = "POST";
        
            await request.SendWebRequest().ToUniTask();
        
            EmergenceUtils.PrintRequestResult("Create Wallet", request);
        
            if (EmergenceUtils.ProcessRequest<string>(request, errorCallback, out var response))
            {
                success?.Invoke();
            }
        }
        
        //Local EVM only
        public async UniTask CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback)
        {
            string url = StaticConfig.APIBase + "createKeyStore" + "?privateKey=" +
                         privateKey + "&password=" + password + "&publicKey=" + publicKey + "&path=" + path;

            using UnityWebRequest request = UnityWebRequest.Post(url, "");
            await request.SendWebRequest().ToUniTask();
            
            EmergenceUtils.PrintRequestResult("Key Store", request);
            if (EmergenceUtils.ProcessRequest<string>(request, errorCallback, out var response))
            {
                success?.Invoke();
            }
        }


        //Local EVM only
        public async UniTask LoadAccount(Account account, LoadAccountSuccess success, ErrorCallback errorCallback)
        {
            string dataString = SerializationHelper.Serialize(account, false);
            string url = StaticConfig.APIBase + "loadAccount";

            using UnityWebRequest request = UnityWebRequest.Post(url, "");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(dataString));
            request.uploadHandler.contentType = "application/json";

            await request.SendWebRequest().ToUniTask();
            EmergenceUtils.PrintRequestResult("Load Account", request);
            if (EmergenceUtils.ProcessRequest<LoadAccountResponse>(request, errorCallback, out var response))
            {
                success?.Invoke();
            }
        }

    }
}