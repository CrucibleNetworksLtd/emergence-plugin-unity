using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class InventoryItemThumbnail : MonoBehaviour
    {
        [SerializeField] 
        private RawImage itemImage;
        
        private bool isPlaying = false;
        
        private List<Texture2D> frames = new List<Texture2D>();
        private List<float> frameDelay = new List<float>();

        private int curFrame = 0;
        
        private float time = 0.0f;
        
        private float minFrameTime = 1f / 60f;
        private float lastFrameTime;

        public void LoadStaticImage(Texture2D texture)
        {
            itemImage.texture = texture;
        }
        
        public void LoadGif(string url, bool autoPlay = true)
        {
            StartCoroutine(SetGifFromUrlCoroutine(url, autoPlay));
        }

        private IEnumerator SetGifFromUrlCoroutine(string imageUrl, bool autoPlay = true)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                Debug.LogError("URL is nothing.");
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(imageUrl))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("File load error.\n" + request.error);
                    yield break;
                }
                
                using( var decoder = new MG.GIF.Decoder(request.downloadHandler.data))
                {
                    var img = decoder.NextImage();
    
                    while( img != null)
                    {
                        // Calculate the time since the last frame
                        float deltaTime = Time.time - lastFrameTime;
                        lastFrameTime = Time.time;
                        
                        frames.Add( img.CreateTexture() );
                        frameDelay.Add( img.Delay / 1000.0f );
                        img = decoder.NextImage();
                        
                        deltaTime -= minFrameTime;
                        
                        if(deltaTime < 0)
                            yield return new WaitForSeconds(Mathf.Max(minFrameTime - deltaTime, 0f));
                    }
                }
                
                if (autoPlay)
                {
                    isPlaying = true;
                    Play();
                }
                else
                {
                    itemImage.texture = frames[0];
                }
            }
        }
        
        private void Play()
        {
            StartCoroutine(PlayCoroutine());
        }

        private IEnumerator PlayCoroutine()
        {
            isPlaying = true;
            curFrame = 0;
            float delay = 0f;
            do
            {
                itemImage.texture = frames[curFrame];
                delay = frameDelay[curFrame];
                yield return new WaitForSeconds(delay);
                curFrame++;
                if (curFrame >= frames.Count)
                {
                    curFrame = 0;
                }
            } while (isPlaying);
        }

    }
}