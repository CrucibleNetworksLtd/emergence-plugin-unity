using System;
using Cysharp.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// Runtime (Build 後と、Editor Playing) での非同期ロードを実現する AwaitCaller.
    /// NOTE: 簡便に実装されたものなので、最適化の余地はある.
    /// </summary>
    public sealed class RuntimeOnlyAwaitCaller : IAwaitCaller
    {
        private readonly NextFrameTaskScheduler _scheduler;
        private readonly float                  _timeOutInSeconds;
        private          float                  _lastTimeoutBaseTime;

        /// <summary>
        /// タイムアウト指定可能なコンストラクタ
        /// </summary>
        /// <param name="timeOutInSeconds">NextFrameIfTimedOutがタイムアウトと見なす時間(秒単位)</param>
        public RuntimeOnlyAwaitCaller(float timeOutInSeconds = 1f / 1000f)
        {
            _scheduler = new NextFrameTaskScheduler();
            _timeOutInSeconds = timeOutInSeconds;
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
            return UniTask.RunOnThreadPool(action);
        }

        public UniTask<T> Run<T>(Func<T> action)
        {
            return UniTask.RunOnThreadPool(action);
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
            return (t - _lastTimeoutBaseTime) >= _timeOutInSeconds;
        }
    }
}
