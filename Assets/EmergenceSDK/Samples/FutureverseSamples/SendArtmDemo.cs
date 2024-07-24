using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Futureverse.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Samples.CoreSamples.DemoStations;
using UnityEngine;

namespace EmergenceSDK.Samples.FutureverseSamples
{
    public class SendArtmDemo : DemoStation<SendArtmDemo>, ILoggedInDemoStation
    {
        [Header("Transaction Details")]
        [SerializeField] [Tooltip("Slot for link to occupy")]
        private string slot = "equippedWith_Engines";

        [SerializeField] [Tooltip("First NFT to link")]
        private string linkA = "did:fv-asset:7672:root:358500:626";

        [SerializeField] [Tooltip("Second NFT to link")]
        private string linkB = "did:fv-asset:7672:root:359524:626";
        
        /// <summary>
        /// This bool is used to determine whether a transaction has been made during this session.
        /// If true the next transaction will be a "Link".
        /// If false the next transaction will be a "Delete Link".
        /// </summary>
        private bool isFirstTransactionOfSession = true;
        
        private IFutureverseService futureverseService;
        private ISessionService sessionService;

        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
                isReady = value;
            }
        }

        private void Start()
        {
            EmergenceServiceProvider.OnServicesLoaded += profile =>
            {
                if (profile != ServiceProfile.Futureverse) return;
                futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
                sessionService = EmergenceServiceProvider.GetService<ISessionService>();
            };

            instructionsGO.SetActive(false);
            IsReady = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        private void Update()
        {
            var futurepassEnabled = sessionService != null && sessionService.HasLoginSetting(LoginSettings.EnableFuturepass);
            if (HasBeenActivated() && IsReady && futurepassEnabled)
            {
                SendTestArtm().Forget();
            }
            else if (IsReady && !futurepassEnabled)
            {
                InstructionsText.text = "You must connect with Futurepass";
            }
            else if (IsReady && futurepassEnabled)
            {
                InstructionsText.text = ActiveInstructions;
            }
        }

        /// <summary>
        /// Using the variables we Generate a transaction request which is associated with the currently connected wallet
        /// </summary>
        private async UniTask SendTestArtm()
        {
            EmergenceLogger.LogInfo("Sending ARTM...", true);

            ArtmTransactionResponse artmTransactionResponse;
            try
            {
                if (isFirstTransactionOfSession)
                {
                    artmTransactionResponse = await futureverseService.SendArtmAsync("An update is being made to your inventory",
                        new List<ArtmOperation>
                            { new(ArtmOperationType.CreateLink, slot, linkA, linkB) }, false);
                }
                else
                {
                    artmTransactionResponse = await futureverseService.SendArtmAsync("An update is being made to your inventory",
                        new List<ArtmOperation>
                            { new(ArtmOperationType.DeleteLink, slot, linkA, linkB) }, false);
                }

                isFirstTransactionOfSession = !isFirstTransactionOfSession;
                
                EmergenceLogger.LogInfo("ARTM successfully sent: " + artmTransactionResponse.TransactionHash, true);
                EmergenceLogger.LogInfo("Retrieving transaction status... ", true);
                var artmStatusAsync = await futureverseService.GetArtmStatusAsync(artmTransactionResponse.TransactionHash);
                EmergenceLogger.LogInfo("ARTM transaction status: " + artmStatusAsync, true);
                if (artmStatusAsync != ArtmStatus.Success)
                {
                    EmergenceLogger.LogInfo("This test is still successful.", true);
                }
            }
            catch
            {
                EmergenceLogger.LogError("Failed sending ARTM", true);
                throw;
            }
        }
    }
}