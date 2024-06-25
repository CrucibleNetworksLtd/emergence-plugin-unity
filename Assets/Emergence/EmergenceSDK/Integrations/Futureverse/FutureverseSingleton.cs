using System;
using EmergenceSDK.Integrations.Futureverse.Internal;
using EmergenceSDK.Internal.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.Integrations.Futureverse
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