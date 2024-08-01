using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Samples.CoreSamples.DemoStations;
using UnityEngine;

namespace EmergenceSDK.Lamina1Demo
{
    public class L1DemoStationController : MonoBehaviour
    {
        private bool IsLoggedIn() => sessionService.IsLoggedIn;
        
        public DemoStation<OpenOverlay> openOverlay;
        public DemoStation<MintAvatar> mintAvatar;
        public DemoStation<L1MintStation> l1MintStation;
        public DemoStation<L1ReadStation> l1ReadStation;

        private List<IDemoStation> stationsRequiringLogin;
        private ISessionService sessionService;

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
            sessionService = EmergenceServiceProvider.GetService<ISessionService>();
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