using System;
using Cysharp.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// Runtime (Build 後と、Editor Playing) での非同期ロードを実現する AwaitCaller.
    /// WebGL など Thread が無いもの向け
    /// </summary>
    public sealed class RuntimeOnlyNoThreadAwaitCaller : IAwaitCaller
    {
        private readonly NextFrameTaskScheduler _scheduler;
        private readonly float                  _timeoutInSeconds;
        private          float                  _lastTimeoutBaseTime;

        /// <summary>
        /// タイムアウト指定可能なコンストラクタ
        /// </summary>
        /// <param name="timeoutInSeconds">NextFrameIfTimedOutがタイムアウトと見なす時間(秒単位)</param>
        public RuntimeOnlyNoThreadAwaitCaller(float timeoutInSeconds = 1f / 1000f)
        {
            _scheduler = new NextFrameTaskScheduler();
            _timeoutInSeconds = timeoutInSeconds;
            ResetLastTimeoutBaseTime();
        }

        public UniTask NextFrame()
        {
            ResetLastTimeoutBaseTime();
            var tcs = new UniTaskCompletionSource<object>();
            _scheduler.Enqueue(() => tcs.TrySetResult(default));
            return tcs.Task;
        }

        public UniTask Run(Action action)
        {
            try
            {
                action();
                return UniTask.FromResult<object>(null);
            }
            catch (Exception ex)
            {
                return UniTask.FromException(ex);
            }
        }

        public UniTask<T> Run<T>(Func<T> action)
        {
            try
            {
                return UniTask.FromResult(action());
            }
            catch (Exception ex)
            {
                return UniTask.FromException<T>(ex);
            }
        }

        public UniTask NextFrameIfTimedOut() => CheckTimeout() ? NextFrame() : UniTask.CompletedTask;

        private void ResetLastTimeoutBaseTime() => _lastTimeoutBaseTime = 0f;

        private bool LastTimeoutBaseTimeNeedsReset => _lastTimeoutBaseTime == 0f;

        private bool CheckTimeout()
        {
            float t = UnityEngine.Time.realtimeSinceStartup;
            if (LastTimeoutBaseTimeNeedsReset)
            {
                _lastTimeoutBaseTime = t;
            }
            return (t - _lastTimeoutBaseTime) >= _timeoutInSeconds;
        }
    }
}