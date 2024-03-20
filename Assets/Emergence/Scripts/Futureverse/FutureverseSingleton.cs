using System;
using EmergenceSDK.Futureverse.Internal;
using EmergenceSDK.Futureverse.Services;
using EmergenceSDK.Futureverse.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using UnityEngine;

namespace EmergenceSDK.Futureverse
{
    public class FutureverseSingleton : SingletonComponent<FutureverseSingleton>
    {
        public enum Environment
        {
            Development,
            Staging,
            Production
        }
        
        public Environment selectedEnvironment;
        public int requestTimeout = 60;
    }
}