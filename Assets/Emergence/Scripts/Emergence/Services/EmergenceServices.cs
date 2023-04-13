using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// The services singleton provides you with all the methods you need to get going with Emergence.
    /// </summary>
    /// <remarks>See our prefabs for examples of how to use it!</remarks>
    public class EmergenceServices : MonoBehaviour
    {
        private static EmergenceServices Instance;

        private bool refreshingToken = false;
        private IAccountService accountService;

        private bool skipWallet = false;
        
        private List<IEmergenceService> services = new List<IEmergenceService>();
        public static T GetService<T>() where T : IEmergenceService
        {
            return Instance.services.OfType<T>().FirstOrDefault();
        }

        private void Awake()
        {
            Instance = this;

            accountService = new AccountService();
            services.Add(accountService);
            services.Add(new PersonaService(accountService));
            services.Add(new AvatarService());
            services.Add(new InventoryService());
            services.Add(new DynamicMetadataService());
            services.Add(new WalletService(accountService));
            services.Add(new QRCodeService());
            services.Add(new ContractService());
            
        }

        private void Update()
        {
            if (ScreenManager.Instance == null)
            {
                return;
            }

            bool uiIsVisible = ScreenManager.Instance.gameObject.activeSelf;

            if (!skipWallet && uiIsVisible && !refreshingToken && accountService.HasAccessToken)
            {
                long now = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (accountService.Expiration.expiresOn - now < 0)
                {
                    refreshingToken = true;
                    ModalPromptOK.Instance.Show("Token expired. Check your wallet for renewal", () =>
                    {
                        accountService.GetAccessToken((token) =>
                        {
                            refreshingToken = false;
                        },
                        (error, code) =>
                        {
                            ErrorLogger.LogError(error, code);
                            refreshingToken = false;
                        });
                    });
                }
            }
        }
        
        public void SkipWallet(bool skip, string accessTokenJson)
        {
            skipWallet = skip;

            BaseResponse<AccessTokenResponse> response = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(accessTokenJson);
            accountService.CurrentAccessToken = SerializationHelper.Serialize(response.message.AccessToken, false);
            accountService.ProcessExpiration(response.message.AccessToken.message);
        }
    }
}