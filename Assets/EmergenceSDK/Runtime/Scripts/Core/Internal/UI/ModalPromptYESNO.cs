using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Runtime.Internal.UI
{
    public class ModalPromptYESNO : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI label;
        public Button yesButton;
        public Button noButton;

        public static ModalPromptYESNO Instance;

        public delegate void ModalPromptYesCallback();
        public delegate void ModalPromptNoCallback();

        private ModalPromptYesCallback yesCallback = null;
        private ModalPromptNoCallback noCallback = null;
        private void Awake()
        {
            Instance = this;
            Hide();
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
        }

        private void OnDestroy()
        {
            yesButton.onClick.RemoveListener(OnYesClicked);
            noButton.onClick.RemoveListener(OnNoClicked);
        }

        public void Show(string title, string question, ModalPromptYesCallback yesCallback = null, ModalPromptNoCallback noCallback = null)
        {
            label.text = "<b>" + title + "</b> " + question;
            gameObject.SetActive(true);
            this.yesCallback = yesCallback;
            this.noCallback = noCallback;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnYesClicked()
        {
            yesCallback?.Invoke();
            Hide();
        }

        private void OnNoClicked()
        {
            noCallback?.Invoke();
            Hide();
        }

    }
}