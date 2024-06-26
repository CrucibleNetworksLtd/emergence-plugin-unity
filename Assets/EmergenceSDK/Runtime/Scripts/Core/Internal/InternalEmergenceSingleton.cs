using System;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Types;

namespace EmergenceSDK.Runtime.Internal
{
    public abstract class InternalEmergenceSingleton : SingletonComponent<EmergenceSingleton>
    {
        protected EmergenceEnvironment? CurrentForcedEnvironment { get; set; }

        /// <summary>
        /// <see cref="IDisposable"/> object that will force a different Emergence environment until disposed.
        /// <remarks>THIS IS A DEVELOPER FEATURE, MEANT ONLY FOR TESTING.<para/>Use with "using" keyword is strongly recommended for easiest management</remarks>
        /// </summary>
        /// <param name="newEnvironment">Environment to force</param>
        /// <returns></returns>
        internal static IDisposable ForcedEnvironment(EmergenceEnvironment newEnvironment) => new ForcedEnvironmentManager(newEnvironment);

        private class ForcedEnvironmentManager : FlagLifecycleManager<EmergenceEnvironment?>
        {
            public ForcedEnvironmentManager(EmergenceEnvironment? newValue) : base(newValue) { }
            protected override EmergenceEnvironment? GetCurrentFlag1Value() => Instance.CurrentForcedEnvironment;
            protected override void SetFlag1Value(EmergenceEnvironment? newValue) => Instance.CurrentForcedEnvironment = newValue;
        }
    }
}