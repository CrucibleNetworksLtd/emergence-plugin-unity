using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;

namespace EmergenceSDK.Integrations.Futureverse
{
    public class FutureverseSingleton : SingletonComponent<FutureverseSingleton>
    {
        public int requestTimeout = 60;
        public EmergenceEnvironment environment;
        public EmergenceEnvironment Environment => ForcedEnvironment ?? environment;
        private EmergenceEnvironment? ForcedEnvironment { get; set; }
        
        internal void RunInForcedEnvironment(EmergenceEnvironment forcedEnvironment, Action action)
        {
            var prevForcedEnvironment = ForcedEnvironment;
            ForcedEnvironment = forcedEnvironment;
            try
            {
                action.Invoke();
            }
            finally
            {
                ForcedEnvironment = prevForcedEnvironment;
            }
        }

        internal async UniTask RunInForcedEnvironmentAsync(EmergenceEnvironment forcedEnvironment, Func<UniTask> action)
        {
            var prevForcedEnvironment = ForcedEnvironment;
            ForcedEnvironment = forcedEnvironment;
            try
            {
                await action();
            }
            finally
            {
                ForcedEnvironment = prevForcedEnvironment;
            }
        }
    }
}