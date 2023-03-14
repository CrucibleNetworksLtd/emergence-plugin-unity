using TMPro;
using UnityEngine;

namespace EmergenceSDK.Internal.UI
{
    public class Tooltip : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI messageText;
        public CanvasGroup cg;

        [Header("Configuration")]
        [Tooltip("In seconds")]
        public float duration = 0.5f;

        private RectTransform rect;

        public static Tooltip Instance
        {
            get;
            private set;
        }

        private enum Directions
        {
            Finished,
            Showing,
            Hiding,
        }

        private Directions state = Directions.Finished;

        private void Awake()
        {
            Instance = this;
            rect = GetComponent<RectTransform>();
            cg.alpha = 0.0f;
        }

        private void Update()
        {
            float alpha = cg.alpha;
            switch (state)
            {
                case Directions.Finished:
                    break;
                case Directions.Showing:
                    alpha += Time.deltaTime / duration;
                    if (alpha >= 1.0f)
                    {
                        alpha = 1.0f;
                        state = Directions.Finished;
                    }
                    break;
                case Directions.Hiding:
                    alpha -= Time.deltaTime / duration;
                    if (alpha <= 0.0f)
                    {
                        alpha = 0.0f;
                        state = Directions.Finished;
                    }
                    break;
            }

            cg.alpha = alpha;
        }

        public void Show(RectTransform target, string message)
        {
            rect.position = target.position;
            messageText.text = message;
            state = Directions.Showing;
        }

        public void Hide()
        {
            state = Directions.Hiding;
        }
    }
}
