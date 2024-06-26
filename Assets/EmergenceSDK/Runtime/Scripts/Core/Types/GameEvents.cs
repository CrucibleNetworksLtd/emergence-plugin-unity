using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Utils;

namespace EmergenceSDK.Runtime.Types
{
    public class GameEvents
    {
        public event Action SyncHandlers;
        public event Func<UniTask> AsyncHandlers;

        public static GameEvents operator +(GameEvents events, Action syncHandler)
        {
            events.SyncHandlers += syncHandler;
            return events;
        }

        public static GameEvents operator -(GameEvents events, Action syncHandler)
        {
            events.SyncHandlers -= syncHandler;
            return events;
        }

        public static GameEvents operator +(GameEvents events, Func<UniTask> asyncHandler)
        {
            events.AsyncHandlers += asyncHandler;
            return events;
        }

        public static GameEvents operator -(GameEvents events, Func<UniTask> asyncHandler)
        {
            events.AsyncHandlers -= asyncHandler;
            return events;
        }

        private void TriggerSyncHandlers()
        {
            SyncHandlers?.Invoke();
            EmergenceLogger.LogInfo("Ran synchronous event handlers");
        }

        private async UniTask TriggerAsyncEventHandlers()
        {
            if (AsyncHandlers != null)
            {
                foreach (var handler in AsyncHandlers.GetInvocationList())
                {
                    await ((Func<UniTask>)handler).Invoke();
                }
            }
            EmergenceLogger.LogInfo("Ran asynchronous event handlers");
        }

        public async UniTask Invoke()
        {
            TriggerSyncHandlers();
            await TriggerAsyncEventHandlers();
        }
    }
}