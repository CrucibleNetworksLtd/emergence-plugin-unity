using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class InventoryItemEntry : MonoBehaviour
    {

        [SerializeField] private RawImage itemImage;
        [SerializeField] private TextMeshProUGUI itemName;
        private string url;
        
        // GIF related variables
        public enum GifState
        {
            None,
            Loading,
            Ready,
            Playing,
            Pause,
        }
        
        public GifState nowState
        {
            get;
            private set;
        }
        
        public int loopCount
        {
            get;
            private set;
        }
        
        public int width
        {
            get;
            private set;
        }

        public int height
        {
            get;
            private set;
        }
        
        // [SerializeField]
        // private UniGifImageAspectController m_imgAspectCtrl;
        // Textures filter mode
        [SerializeField]
        private FilterMode m_filterMode = FilterMode.Point;
        // Textures wrap mode
        [SerializeField]
        private TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        // Load from url on start
        [SerializeField]
        private bool m_loadOnStart;
        // GIF image url (WEB or StreamingAssets path)
        [SerializeField]
        private string m_loadOnStartUrl;
        // Rotating on loading
        [SerializeField]
        private bool m_rotateOnLoading;
        // Debug log flag
        [SerializeField]
        private bool m_outputDebugLog;
        
        // Delay time
        private float m_delayTime;
        // Texture index
        private int m_gifTextureIndex;
        // loop counter
        private int m_nowLoopCount;
        private List<UniGif.GifTexture> m_gifTextureList;

        private InventoryItem item;

        private void Awake()
        {
            RequestImage.Instance.OnImageReady += Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed += Instance_OnImageFailed;
        }
        
        private void OnDestroy()
        {
            RequestImage.Instance.OnImageReady -= Instance_OnImageReady;
            RequestImage.Instance.OnImageFailed -= Instance_OnImageFailed;
        }

        public void SetItem(InventoryItem item)
        {
            this.item = item;
            
            itemName.text = item?.meta?.name;
            SetImageUrl(item?.meta?.content?.First().url);
        }

        public void SetImageUrl(string imageUrl)
        {
            // Debug.Log("Image url: " + url);
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.ToLower().Contains("gif")) {
                
                // Handle GIFs
                StartCoroutine(SetGifFromUrlCoroutine(imageUrl));
            }
            else {
                RequestImage.Instance.AskForImage(imageUrl);
            }
        }
        
        private void Instance_OnImageReady(string _url, Texture2D texture)
        {
            if (_url.Equals(item?.meta?.content?.First().url))
            {
                itemImage.texture = texture;
            }
        }

        private void Instance_OnImageFailed(string imageUrl, string error, long errorCode)
        {
            Debug.LogError("[" + imageUrl + "] " + error + " " + errorCode);
        }
        
        private IEnumerator SetGifFromUrlCoroutine(string imageUrl, bool autoPlay = true)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("URL is nothing.");
            yield break;
        }

        if (nowState == GifState.Loading)
        {
            Debug.LogWarning("Already loading.");
            yield break;
        }
        nowState = GifState.Loading;
        
        using (UnityWebRequest request = UnityWebRequest.Get(imageUrl))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("File load error.\n" + request.error);
                nowState = GifState.None;
                yield break;
            }

            Clear();
            nowState = GifState.Loading;

            // Get GIF textures
            yield return StartCoroutine(UniGif.GetTextureListCoroutine(request.downloadHandler.data, (gifTexList, loopCount, width, height) =>
                {
                    if (gifTexList != null)
                    {
                        m_gifTextureList = gifTexList;
                        this.loopCount = loopCount;
                        this.width = width;
                        this.height = height;
                        nowState = GifState.Ready;

                        // m_imgAspectCtrl.FixAspectRatio(width, height);

                        if (m_rotateOnLoading)
                        {
                            transform.localEulerAngles = Vector3.zero;
                        }

                        if (autoPlay)
                        {
                            Play();
                        }
                    }
                    else
                    {
                        Debug.LogError("Gif texture get error.");
                        nowState = GifState.None;
                    }
                },
                m_filterMode, m_wrapMode, m_outputDebugLog));
        }
    }
        
        public void Clear()
        {
            if (itemImage != null)
            {
                itemImage.texture = null;
            }

            if (m_gifTextureList != null)
            {
                for (int i = 0; i < m_gifTextureList.Count; i++)
                {
                    if (m_gifTextureList[i] != null)
                    {
                        if (m_gifTextureList[i].m_texture2d != null)
                        {
                            Destroy(m_gifTextureList[i].m_texture2d);
                            m_gifTextureList[i].m_texture2d = null;
                        }
                        m_gifTextureList[i] = null;
                    }
                }
                m_gifTextureList.Clear();
                m_gifTextureList = null;
            }

            nowState = GifState.None;
        }

        /// <summary>
        /// Play GIF animation
        /// </summary>
        public void Play()
        {
            if (nowState != GifState.Ready)
            {
                Debug.LogWarning("State is not READY.");
                return;
            }
            if (itemImage == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
            {
                Debug.LogError("Raw Image or GIF Texture is nothing.");
                return;
            }
            nowState = GifState.Playing;
            itemImage.texture = m_gifTextureList[0].m_texture2d;
            m_delayTime = Time.time + m_gifTextureList[0].m_delaySec;
            m_gifTextureIndex = 0;
            m_nowLoopCount = 0;
        }
        
        private void Update()
        {
            switch (nowState)
            {
                case GifState.None:
                    break;

                case GifState.Loading:
                    if (m_rotateOnLoading)
                    {
                        transform.Rotate(0f, 0f, 30f * Time.deltaTime, Space.Self);
                    }
                    break;

                case GifState.Ready:
                    break;

                case GifState.Playing:
                    if (itemImage == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
                    {
                        return;
                    }
                    if (m_delayTime > Time.time)
                    {
                        return;
                    }
                    // Change texture
                    m_gifTextureIndex++;
                    if (m_gifTextureIndex >= m_gifTextureList.Count)
                    {
                        m_gifTextureIndex = 0;

                        if (loopCount > 0)
                        {
                            m_nowLoopCount++;
                            if (m_nowLoopCount >= loopCount)
                            {
                                // Stop();
                                return;
                            }
                        }
                    }
                    itemImage.texture = m_gifTextureList[m_gifTextureIndex].m_texture2d;
                    m_delayTime = Time.time + m_gifTextureList[m_gifTextureIndex].m_delaySec;
                    break;

                case GifState.Pause:
                    break;

                default:
                    break;
            }
        }

    }
}
