using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public class RequestImage : MonoBehaviour
    {
        public Texture2D DefaultThumbnail;
        public Texture2D DownloadingThumbnail;
        
        public static RequestImage Instance { get; private set; }

        private Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        
        private CancellationTokenSource cancellationTokenSource;
        
        public delegate void DownloadReady(string url, Texture2D texture, DownloadImage self);
        public delegate void DownloadFailed(string url, string error, long errorCode);

        public delegate void ImageReady(string url, Texture2D texture);
        public delegate void ImageFailed(string error, long errorCode);
        public event ImageReady OnImageReady;

        private Queue<string> urlQueue = new Queue<string>();
        private GenericPool<DownloadImage> pool = new GenericPool<DownloadImage>(5, 1);

        /// <summary>
        /// This method returns the texture with events
        /// </summary>
        public bool AskForImage(string url)
        {
            if (url == null)
            {
                return false;
            }
            
            if (cachedTextures.ContainsKey(url) && cachedTextures[url] != null)
            {
                OnImageReady?.Invoke(url, cachedTextures[url]);
            }
            else
            {
                urlQueue.Enqueue(url);
            }

            return true;
        }

        public void AskForDefaultImage()
        {
            OnImageReady?.Invoke(null, DefaultThumbnail);
        }

        private class CallbackContainer
        {
            public ImageReady imageReadyCallback;
            public ImageFailed imageFailedCallback;
        }

        private Dictionary<string, CallbackContainer> callbackCache = new Dictionary<string, CallbackContainer>();

        /// <summary>
        /// This method returns the texture in delegate callback, compatible with promises
        /// </summary>
        public void AskForImage(string url, ImageReady imageReadyCallback, ImageFailed imageFailedCallback)
        {
            if (cachedTextures.ContainsKey(url) && cachedTextures[url] != null)
            {
                imageReadyCallback?.Invoke(url, cachedTextures[url]);
            }
            else
            {
                if (!callbackCache.ContainsKey(url))
                {
                    CallbackContainer callbacks = new CallbackContainer()
                    {
                        imageReadyCallback = imageReadyCallback,
                        imageFailedCallback = imageFailedCallback,
                    };

                    callbackCache.Add(url, callbacks);
                    urlQueue.Enqueue(url);
                }
            }
        }
        
        private void HandleImageDownloadSuccess(string url, Texture2D texture, DownloadImage self)
        {
            cachedTextures[url] = texture;

            if (callbackCache.ContainsKey(url))
            {
                callbackCache[url].imageReadyCallback?.Invoke(url, texture);
                callbackCache.Remove(url);
            }
            else
            {
                OnImageReady?.Invoke(url, texture);
            }
            
            pool.ReturnUsedObject(ref self);
        }

        private void HandleImageDownloadFailure(string url, string error, long errorCode)
        {
            // Add the DefaultThumbnail to the cache with the failed URL
            cachedTextures[url] = DefaultThumbnail;
            if (callbackCache.ContainsKey(url))
            {
                callbackCache[url].imageFailedCallback?.Invoke(url + " : " + error, errorCode);
                callbackCache.Remove(url);
            }
            // Supply DefaultThumbnail as the texture when there's a failure
            OnImageReady?.Invoke(url, DefaultThumbnail);
        }
        
        private async UniTaskVoid ProcessUrlQueue()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (urlQueue.Count > 0)
                {
                    string currentUrl = urlQueue.Dequeue();

                    if (!cachedTextures.ContainsKey(currentUrl))
                    {
                        cachedTextures.Add(currentUrl, null);
                        DownloadImage(currentUrl);
                    }
                }

                await UniTask.DelayFrame(1, cancellationToken: cancellationTokenSource.Token);
            }
        }

        private async void DownloadImage(string url)
        {
            await pool.GetNewObject().Download(this, url, HandleImageDownloadSuccess, HandleImageDownloadFailure);
        }

        private void Awake()
        {
            Instance = this;
            cancellationTokenSource = new CancellationTokenSource();
        }

        private void Start()
        {
            ProcessUrlQueue().Forget();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}