using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Internal.UI
{
    public class ModalCancel : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Button cancelButton;

        public static ModalCancel Instance;

        public delegate void ModalPromptCancelCallback();

        private ModalPromptCancelCallback callback = null;

        private void Awake()
        {
            Instance = this;
            Hide();
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(string message, ModalPromptCancelCallback cancelCallback = default)
        {
            label.text = message;
            gameObject.SetActive(true);
            callback = cancelCallback;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnCancelClicked()
        {
            callback?.Invoke();
        }
    }
}