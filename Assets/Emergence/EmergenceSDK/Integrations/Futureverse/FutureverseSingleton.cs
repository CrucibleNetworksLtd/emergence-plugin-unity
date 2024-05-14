using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;

namespace EmergenceSDK.Integrations.Futureverse
{
    public class FutureverseSingleton : SingletonComponent<FutureverseSingleton>
    {
        public int requestTimeout = 60;
        public EmergenceEnvironment environment;
        public EmergenceEnvironment Environment => CurrentForcedEnvironment ?? environment;
        private EmergenceEnvironment? CurrentForcedEnvironment { get; set; }
        
        internal IDisposable ForcedEnvironment(EmergenceEnvironment? newEnvironment) => new ForcedEnvironmentManager(newEnvironment);
        
        private class ForcedEnvironmentManager : FlagLifecycleManager<EmergenceEnvironment?>
        {
            public ForcedEnvironmentManager(EmergenceEnvironment? newValue) : base(newValue) { }
            protected override EmergenceEnvironment? GetCurrentFlag1Value() => Instance.CurrentForcedEnvironment;
            protected override void SetFlag1Value(EmergenceEnvironment? newValue) => Instance.CurrentForcedEnvironment = newValue;
        }
    }
}