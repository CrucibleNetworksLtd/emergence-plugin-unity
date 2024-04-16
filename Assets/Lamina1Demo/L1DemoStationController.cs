using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Lamina1Demo;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public class L1DemoStationController : MonoBehaviour
    {
        private bool IsLoggedIn() => sessionServiceInternal.CurrentAccessToken.Length != 0;
        
        public DemoStation<OpenOverlay> openOverlay;
        public DemoStation<MintAvatar> mintAvatar;
        public DemoStation<L1MintStation> l1MintStation;
        public DemoStation<L1ReadStation> l1ReadStation;

        private List<IDemoStation> stationsRequiringLogin;
        private ISessionServiceInternal sessionServiceInternal;

        public async void Awake()
        {
            stationsRequiringLogin = new List<IDemoStation>()
            {
                mintAvatar as IDemoStation,
                l1MintStation as IDemoStation,
                l1ReadStation as IDemoStation,
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