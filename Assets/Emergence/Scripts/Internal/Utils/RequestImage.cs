using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Internal.Utils
{
    public class RequestImage : MonoBehaviour
    {
        public static RequestImage Instance { get; private set; }

        private Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        private delegate void DownloadReady(string url, Texture2D texture, DownloadImage self);
        private delegate void DownloadFailed(string url, string error, long errorCode);

        public delegate void ImageReady(string url, Texture2D texture);
        public delegate void ImageFailed(string url, string error, long errorCode);
        public event ImageReady OnImageReady;
        public event ImageFailed OnImageFailed;

        private Queue<string> urlQueue = new Queue<string>();
        private GenericPool<DownloadImage> pool = new GenericPool<DownloadImage>(5, 1);

        /// <summary>
        /// This method returns the texture with events
        /// </summary>
        public bool AskForImage(string url)
        {
            bool result = true;
            if (url == null)
            {
                // Debug.LogWarning("RequestImage: URL cannot be null");
                result = false;
            }
            else if (cachedTextures.ContainsKey(url) && cachedTextures[url] != null)
            {
                OnImageReady?.Invoke(url, cachedTextures[url]);
            }
            else
            {
                urlQueue.Enqueue(url);
            }

            return result;
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

        #region Monobehaviour

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (urlQueue.Count > 0)
            {
                string currentUrl = urlQueue.Dequeue();

                if (!cachedTextures.ContainsKey(currentUrl))
                {
                    cachedTextures.Add(currentUrl, null);

                    pool.GetNewObject().Download(this, currentUrl,
                        (url, texture, self) =>
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
                        },
                        (url, error, errorCode) =>
                        {
                            cachedTextures.Remove(url);
                            if (callbackCache.ContainsKey(url))
                            {
                                callbackCache[url].imageFailedCallback?.Invoke(url, error, errorCode);
                                callbackCache.Remove(url);
                            }
                            else
                            {
                                OnImageFailed?.Invoke(url, error, errorCode);
                            }
                        });
                }
            }
        }

        #endregion Monobehaviour

        private class DownloadImage
        {
            UnityWebRequest request = null;
            DownloadReady successCallback = null;
            DownloadFailed failedCallback = null;

            public void Download(RequestImage ri, string url, DownloadReady success, DownloadFailed failed)
            {
                request = UnityWebRequestTexture.GetTexture(url);
                successCallback = success;
                failedCallback = failed;

                ri.StartCoroutine(MakeRequest());
            }

            private IEnumerator MakeRequest()
            {
                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, 0, false);
                    bool error = false;

                    if (request.downloadedBytes > 0)
                    {
                        try
                        {
                            texture = DownloadHandlerTexture.GetContent(request);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e.Message);
                            error = true;
                        }

                        if (texture == null)
                        {
                            Debug.LogWarning("Couldn't convert downloaded image at " + request.url);
                            failedCallback?.Invoke(request.url, "Couldn't convert downloaded image", 0);
                        }
                    }
                    else
                    {
                        error = true;
                    }

                    if (error)
                    {
                        // Transparent texture
                        texture.SetPixels(new Color[]
                        {
                            new Color(0,0,0,0),
                            new Color(0,0,0,0),
                            new Color(0,0,0,0),
                            new Color(0,0,0,0),
                        });

                        texture.Apply();
                    }

                    successCallback?.Invoke(request.url, texture, this);
                }
                else
                {
                    failedCallback?.Invoke(request.url, request.error, request.responseCode);
                    Debug.LogWarning("Failed to download image at " + request.url);
                }

                request = null;
            }
        }
    }
}