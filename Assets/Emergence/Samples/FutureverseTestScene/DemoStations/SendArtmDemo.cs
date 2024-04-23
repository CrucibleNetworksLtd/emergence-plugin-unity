using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

namespace EmergenceSDK.Samples.FutureverseTestScene.DemoStations
{
    public class SendArtmDemo : DemoStation<SendArtmDemo>, ILoggedInDemoStation
    {
        private IFutureverseService futureverseService;

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
            futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();

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
            if (HasBeenActivated() && IsReady && futureverseService.UsingFutureverse)
            {
                SendTestArtm().Forget();
            }
            else if (IsReady && !futureverseService.UsingFutureverse)
            {
                InstructionsText.text = "You must connect with Futurepass";
            }
            else if (IsReady && futureverseService.UsingFutureverse)
            {
                InstructionsText.text = ActiveInstructions;
            }
        }

        private async UniTask SendTestArtm()
        {
            EmergenceLogger.LogInfo("Sending ARTM...", true);

            try
            {
                var artmTransactionResponse = await futureverseService.SendArtmAsync("An update is being made to your inventory",
                    new List<FutureverseArtmOperation>
                        { new(FutureverseArtmOperationType.DeleteLink, "equippedWith_Engines", "did:fv-asset:7672:root:358500:626", "did:fv-asset:7672:root:359524:626") }, false);
                
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