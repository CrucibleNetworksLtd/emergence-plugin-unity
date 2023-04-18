using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Services;
using UnityEngine;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// The services singleton provides you with all the methods you need to get going with Emergence.
    /// </summary>
    /// <remarks>See our prefabs for examples of how to use it!</remarks>
    public class EmergenceServices : MonoBehaviour
    {
        private static EmergenceServices instance;

        private bool refreshingToken = false;
        private ISessionService sessionService;
        private List<IEmergenceService> services = new List<IEmergenceService>();
        
        public static T GetService<T>() where T : IEmergenceService
        {
            return instance.services.OfType<T>().FirstOrDefault();
        }

        private void Awake()
        {
            instance = this;

            sessionService = new SessionService();
            services.Add(sessionService);
            services.Add(new PersonaService(sessionService));
            services.Add(new AvatarService());
            services.Add(new InventoryService());
            services.Add(new DynamicMetadataService());
            services.Add(new WalletService(sessionService));
            services.Add(new QRCodeService());
            services.Add(new ContractService());
        }
    }
}