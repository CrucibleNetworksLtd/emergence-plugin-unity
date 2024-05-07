using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Integrations.Futureverse.Internal;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
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
            ClearServices();
            AddService(new AvatarService());
            AddService(new InventoryService());
            AddService(new DynamicMetadataService());
            AddService(new ContractService());
            AddService(new ChainService());
            AddService(new FutureverseService());
            var sessionServiceInternal = (ISessionServiceInternal)AddService(new SessionService());
            AddService(new WalletService(sessionServiceInternal));
            AddService(new PersonaService(sessionServiceInternal));
        }

        private EmergenceServiceProvider()
        {
            LoadServices();
        }

        internal IEmergenceService AddService(IEmergenceService service)
        {
            _services.Add(service);
            return service;
        }

        internal void ClearServices()
        {
            _services.Clear();
        }
        
        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to request, must implement <see cref="IEmergenceService"/></typeparam>
        /// <returns>The requested service or null if not found</returns>
        public static T GetService<T>() where T : IEmergenceService
        {
            return Instance._services.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Gets all the services of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to request, must implement <see cref="IEmergenceService"/></typeparam>
        /// <returns>A <see cref="List{T}"/> of the matching services</returns>
        public static List<T> GetServices<T>()
        {
            return Instance._services.OfType<T>().ToList();
        }

        internal static void Load()
        {
            _instance = new EmergenceServiceProvider();
        }

        internal static void Unload()
        {
            _instance = null;
        }
    }
}