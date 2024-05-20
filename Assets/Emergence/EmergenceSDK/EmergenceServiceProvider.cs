using System;
using System.Collections.Generic;
using System.Threading;
using EmergenceSDK.Extensions.Threading;
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
    public static class EmergenceServiceProvider
    {
        public static ServiceProfile? LoadedProfile { get; private set; }
        public static event Action<ServiceProfile> OnServicesLoaded;
        public static event Action OnServicesUnloaded;
        
        private static readonly List<IEmergenceService> Services = new();
        private static readonly ReaderWriterLockSlim Lock = new();
        
        private static void LoadFutureverseServices()
        {
            AddService(new AvatarService());
            var sessionService = AddService(new SessionService());
            var walletService = AddService(new WalletService(sessionService));
            var futureverseService = AddService(new FutureverseService(walletService));
            AddService(new FutureverseInventoryService(futureverseService));
            AddService(new DynamicMetadataService());
            AddService(new ContractService());
            AddService(new ChainService());
            AddService(new PersonaService(sessionService));
        }

        private static void LoadDefaultServices()
        {
            AddService(new AvatarService());
            AddService(new InventoryService());
            AddService(new DynamicMetadataService());
            AddService(new ContractService());
            AddService(new ChainService());
            var sessionService = AddService(new SessionService());
            AddService(new WalletService(sessionService));
            AddService(new PersonaService(sessionService));
        }

        private static T AddService<T>(T service) where T : class, IEmergenceService
        {
            if (typeof(T).IsInterface || typeof(T).IsAbstract)
            {
                throw new ArgumentException($"The type {typeof(T).Name} must be a concrete class.");
            }

            foreach (var existingService in Services)
            {
                if (existingService.GetType() == service.GetType())
                {
                    throw new InvalidOperationException($"Service of type {service.GetType().Name} is already added.");
                }
            }

            Services.Add(service);
            return service;
        }

        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to request, must implement <see cref="IEmergenceService"/>.</typeparam>
        /// <returns>The requested service or null if not found.</returns>
        public static T GetService<T>() where T : class, IEmergenceService
        {
            using IDisposable readLock = Lock.ReadLock();
            foreach (var service in Services)
            {
                if (service is T typedService)
                {
                    return typedService;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all the services of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to request, must implement <see cref="IEmergenceService"/>.</typeparam>
        /// <returns>A <see cref="List{T}"/> of the matching services.</returns>
        public static List<T> GetServices<T>() where T : class, IEmergenceService
        {
            using IDisposable readLock = Lock.ReadLock();
            List<T> services = new List<T>();
            foreach (var service in Services)
            {
                if (service is T typedService)
                {
                    services.Add(typedService);
                }
            }
            return services;
        }

        internal static void Load(ServiceProfile profile)
        {
            using (Lock.WriteLock())
            {
                Services.Clear();

                if (profile == ServiceProfile.Default)
                    LoadDefaultServices();
                else if (profile == ServiceProfile.Futureverse)
                    LoadFutureverseServices();
                else
                    throw new ArgumentOutOfRangeException(nameof(profile), profile, null);

                LoadedProfile = profile;
            }

            OnServicesLoaded?.Invoke(profile);
        }

        internal static void Unload()
        {
            using (Lock.WriteLock())
            {
                Services.Clear();
                LoadedProfile = null;
            }

            OnServicesUnloaded?.Invoke();
        }
    }

    public enum ServiceProfile
    {
        Default,
        Futureverse
        // Add more profiles as needed
    }
}
