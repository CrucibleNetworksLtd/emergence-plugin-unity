using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class Modal : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Image clickBlocker;
        public static Modal Instance;

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show(string message, bool captureClicks = true)
        {
            label.text = message;
            gameObject.SetActive(true);
            clickBlocker.raycastTarget = captureClicks;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}