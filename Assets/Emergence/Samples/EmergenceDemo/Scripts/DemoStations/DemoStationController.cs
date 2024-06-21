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
        private bool IsLoggedIn() => sessionService is { IsLoggedIn: true };
        
        public DemoStation<OpenOverlay> openOverlay;

        private readonly List<ILoggedInDemoStation> stationsRequiringLogin = new ();
        private IDemoStation[] stations;
        private ISessionService sessionService;

        public async void Awake()
        {
            stations = gameObject.GetComponentsInChildren<IDemoStation>();
            foreach (var station in stations)
            {
                if (station is ILoggedInDemoStation loggedInDemoStation)
                {
                    stationsRequiringLogin.Add(loggedInDemoStation);
                }
                
                //OpenOverlay is the first station, so we can set it to ready here
                if (station is DemoStation<OpenOverlay>)
                {
                    station.IsReady = true;
                }
            }
            
            await UniTask.WaitUntil(IsLoggedIn);
            ActivateStations();
        }

        public void Start()
        {
            EmergenceServiceProvider.OnServicesLoaded += _ => sessionService = EmergenceServiceProvider.GetService<ISessionService>();
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