using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Futureverse.Internal;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK
{
    /// <summary>
    /// The services singleton provides you with all the methods you need to get going with Emergence.
    /// </summary>
    /// <remarks>See our prefabs for examples of how to use it!</remarks>
    public class EmergenceServiceProvider : MonoBehaviour
    {
        private static EmergenceServiceProvider instance;

        private List<IEmergenceService> services = new List<IEmergenceService>();
        
        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        public static T GetService<T>() where T : IEmergenceService
        {
            return instance.services.OfType<T>().FirstOrDefault();
        }

        private void Awake()
        {
            instance = this;

            var personaService = new PersonaService();
            services.Add(personaService);
            var sessionService = new SessionService(personaService);
            services.Add(sessionService);
            services.Add(new AvatarService());
            services.Add(new InventoryService());
            services.Add(new DynamicMetadataService());
            services.Add(new WalletService(personaService, sessionService));
            services.Add(new ContractService());
            services.Add(new ChainService());
            services.Add(new FutureverseService());
        }
    }
}