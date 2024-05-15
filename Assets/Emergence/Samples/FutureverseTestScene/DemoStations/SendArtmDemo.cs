using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Implementations.Login.Types;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using UnityEngine;

namespace EmergenceSDK.Samples.FutureverseTestScene.DemoStations
{
    public class SendArtmDemo : DemoStation<SendArtmDemo>, ILoggedInDemoStation
    {
        private IFutureverseService _futureverseService;
        private ISessionService _sessionService;

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
            _futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            _sessionService = EmergenceServiceProvider.GetService<ISessionService>();

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
            var futurepassEnabled = _sessionService.HasLoginSetting(LoginSettings.EnableFuturepass);
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

        private async UniTask SendTestArtm()
        {
            EmergenceLogger.LogInfo("Sending ARTM...", true);

            try
            {
                var artmTransactionResponse = await _futureverseService.SendArtmAsync("An update is being made to your inventory",
                    new List<ArtmOperation>
                        { new(ArtmOperationType.DeleteLink, "equippedWith_Engines", "did:fv-asset:7672:root:358500:626", "did:fv-asset:7672:root:359524:626") }, false);
                
                EmergenceLogger.LogInfo("ARTM successfully sent: " + artmTransactionResponse.TransactionHash, true);
                EmergenceLogger.LogInfo("Retrieving transaction status... ", true);
                var artmStatusAsync = await _futureverseService.GetArtmStatusAsync(artmTransactionResponse.TransactionHash);
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