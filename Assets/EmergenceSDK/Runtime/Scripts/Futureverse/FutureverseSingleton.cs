using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Types;
using UnityEngine;

namespace EmergenceSDK.Runtime.Futureverse
{
    public sealed class FutureverseSingleton : InternalFutureverseSingleton
    {
        /// <summary>
        /// Timeout in seconds for all requests to Futureverse-owned endpoints
        /// </summary>
        public int RequestTimeout => requestTimeout;
        public EmergenceEnvironment Environment => CurrentForcedEnvironment ?? environment;

        [SerializeField]
        private int requestTimeout = 60;

        [SerializeField]
        private EmergenceEnvironment environment;
    }
}