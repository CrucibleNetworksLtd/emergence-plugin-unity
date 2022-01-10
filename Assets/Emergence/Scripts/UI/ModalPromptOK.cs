using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
{
    public class ModalPromptOK : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public CanvasGroup cg;
        public Button okButton;

        public static ModalPromptOK Instance;

        public delegate void ModalPromptOkCallback();

        private ModalPromptOkCallback callback = null;

        private void Awake()
        {
            Instance = this;
            Hide();
            okButton.onClick.AddListener(OnOkClicked);
        }

        public void Show(string message, ModalPromptOkCallback callback = null)
        {
            cg.alpha = 0.5f;
            label.text = message;
            gameObject.SetActive(true);
            this.callback = callback;
        }

        public void Hide()
        {
            cg.alpha = 1.0f;
            gameObject.SetActive(false);
        }

        private void OnOkClicked()
        {
            callback?.Invoke();
            Hide();
        }
    }
}