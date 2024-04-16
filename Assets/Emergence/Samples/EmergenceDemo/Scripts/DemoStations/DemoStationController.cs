using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class DemoStationController : MonoBehaviour
    {
        private bool IsLoggedIn() => sessionServiceInternal.CurrentAccessToken.Length != 0;
        
        public DemoStation<OpenOverlay> openOverlay;
        
        public DemoStation<DynamicMetadataController> dynamicMetadataController;
        public DemoStation<InventoryDemo> inventoryService;
        public DemoStation<MintAvatar> mintAvatar;
        public DemoStation<ReadMethod> readMethod;
        public DemoStation<SignMessage> signMessage;
        public DemoStation<WriteMethod> writeMethod;

        private List<IDemoStation> stationsRequiringLogin;
        private ISessionServiceInternal sessionServiceInternal;

        public async void Awake()
        {
            stationsRequiringLogin = new List<IDemoStation>()
            {
                dynamicMetadataController as IDemoStation,
                inventoryService as IDemoStation,
                mintAvatar as IDemoStation,
                readMethod as IDemoStation,
                signMessage as IDemoStation,
                writeMethod as IDemoStation
            };

            //OpenOverlay is the first station, so we can set it to ready here
            (openOverlay as IDemoStation).IsReady = true;
            
            await UniTask.WaitUntil(IsLoggedIn);
            ActivateStations();
        }

        public void Start()
        {
            sessionServiceInternal = EmergenceServiceProvider.GetService<ISessionServiceInternal>();
        }

        private void ActivateStations()
        {
            EmergenceLogger.LogInfo("Activating stations", true);
            foreach (var station in stationsRequiringLogin)
            {
                station.IsReady = true;
            }
        }
    }
}