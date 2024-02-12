using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class ModalPromptOK : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public CanvasGroup cg;
        public Button okButton;
        public Image clickBlocker;

        public static ModalPromptOK Instance;

        public delegate void ModalPromptOkCallback();

        private ModalPromptOkCallback callback = null;

        private void Awake()
        {
            Instance = this;
            Hide();
            okButton.onClick.AddListener(OnOkClicked);
        }

        private void OnDestroy()
        {
            okButton.onClick.RemoveListener(OnOkClicked);
        }

        public void Show(string message, ModalPromptOkCallback callback = null, bool captureClicks = true)
        {
            label.text = message;
            gameObject.SetActive(true);
            this.callback = callback;
            clickBlocker.raycastTarget = captureClicks;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnOkClicked()
        {
            callback?.Invoke();
            Hide();
        }
    }
}