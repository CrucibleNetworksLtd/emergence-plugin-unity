using System;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Internal.Utils;

namespace EmergenceSDK.Runtime.Internal.Types
{
    internal abstract class FlagLifecycleManager<T1> : IDisposable
    {
        protected readonly List<Action> ResetActions = new();
        protected abstract T1 GetCurrentFlag1Value();
        protected abstract void SetFlag1Value(T1 newValue);

        protected FlagLifecycleManager(T1 newValue1)
        {
            var previousFlag1State = GetCurrentFlag1Value();
            SetFlag1Value(newValue1);
            ResetActions.Add(() => { SetFlag1Value(previousFlag1State);});
        }
            
        ~FlagLifecycleManager()
        {
            Dispose();
            using (EmergenceLogger.VerboseOutput(false))
            {
                EmergenceLogger.LogWarning($"Did not manually dispose {GetType().Name}! Disposed by GC.");
            }
        }

        private void ResetFlags()
        {
            foreach (var action in ResetActions)
            {
                action();
            }
        }
        
        public void Dispose()
        {
            ResetFlags();
            ResetActions.Clear();
            GC.SuppressFinalize(this);
        }
    }
    internal abstract class FlagLifecycleManager<T1, T2> : FlagLifecycleManager<T1>
    {
        protected abstract T2 GetCurrentFlag2Value();
        protected abstract void SetFlag2Value(T2 newValue);

        protected FlagLifecycleManager(T1 newValue1, T2 newValue2) : base(newValue1)
        {
            var previousFlag2State = GetCurrentFlag2Value();
            SetFlag2Value(newValue2);
            ResetActions.Add(() => { SetFlag2Value(previousFlag2State);});
        }
    }
}