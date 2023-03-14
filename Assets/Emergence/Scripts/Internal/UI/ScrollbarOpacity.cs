using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class ScrollbarOpacity : MonoBehaviour
    {
        public float duration = 1.0f;
        public float delay = 1.0f;
        public Image handle;
        private float timeCounter = 0.0f;

        private float originalAlpha = 0.5f;

        private void Start()
        {
            originalAlpha = handle.color.a;

            Color color = handle.color;
            color.a = 0.0f;
            handle.color = color;
        }

        public void OnValueChanged()
        {
            timeCounter = 0.0f;

            Color color = handle.color;
            color.a = originalAlpha;

            handle.color = color;
        }

        private void Update()
        {
            timeCounter += Time.deltaTime;
            if (timeCounter > delay)
            {
                Color color = handle.color;
                color.a = color.a - Time.deltaTime / duration;

                handle.color = color;
            }
        }
    }
}