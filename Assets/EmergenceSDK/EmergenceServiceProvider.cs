using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Integrations.Futureverse.Internal;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Services;

namespace EmergenceSDK
{
    /// <summary>
    /// The services singleton provides you with all the methods you need to get going with Emergence.
    /// </summary>
    /// <remarks>See our prefabs for examples of how to use it!</remarks>
    public class EmergenceServiceProvider
    {
        private readonly List<IEmergenceService> _services = new();
        private static EmergenceServiceProvider _instance;

        public static EmergenceServiceProvider Instance
        {
            get => _instance ??= new EmergenceServiceProvider();
            set => _instance = value;
        }

        private void LoadServices()
        {
            _services.Clear();
            var personaService = new PersonaService();
            _services.Add(personaService);
            var sessionService = new SessionService(personaService);
            _services.Add(sessionService);
            _services.Add(new AvatarService());
            _services.Add(new InventoryService());
            _services.Add(new DynamicMetadataService());
            _services.Add(new WalletService(personaService, sessionService));
            _services.Add(new ContractService());
            _services.Add(new ChainService());
            _services.Add(new FutureverseService());
        }

        private EmergenceServiceProvider()
        {
            LoadServices();
        }

        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        public static T GetService<T>() where T : IEmergenceService
        {
            return Instance._services.OfType<T>().FirstOrDefault();
        }

        public static void Load()
        {
            _instance = new EmergenceServiceProvider();
        }

        public static void Unload()
        {
            _instance = null;
        }
    }
}