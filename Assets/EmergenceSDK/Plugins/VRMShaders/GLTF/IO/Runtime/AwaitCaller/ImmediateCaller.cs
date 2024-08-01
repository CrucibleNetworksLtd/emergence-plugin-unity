using System;
using Cysharp.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// 同期実行
    /// </summary>
    public sealed class ImmediateCaller : IAwaitCaller
    {
        public Exception Exception { get; set; }

        public UniTask NextFrame()
        {
            return UniTask.FromResult<object>(null);
        }

        public UniTask Run(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Exception = e;
                return UniTask.FromException(e);
            }
            
            return UniTask.FromResult<object>(null);
        }

        public UniTask<T> Run<T>(Func<T> action)
        {
            try
            {
                return UniTask.FromResult(action());
            }
            catch (Exception e)
            {
                Exception = e;
                return UniTask.FromException<T>(e);
            }
        }

        public UniTask NextFrameIfTimedOut() => NextFrame();
    }
}