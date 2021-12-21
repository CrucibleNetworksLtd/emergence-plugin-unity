using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class ModalPromptYESNO : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI label;
        public Button yesButton;
        public Button noButton;
        public Image bgBlurredImage;

        public static ModalPromptYESNO Instance;

        public delegate void ModalPromptYesCallback();

        private ModalPromptYesCallback callback = null;
        private void Awake()
        {
            Instance = this;
            Hide();
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
        }

        public void Show(string title, string question, ModalPromptYesCallback callback = null)
        {
            label.text = "<b>" + title + "</b> " + question;
            gameObject.SetActive(true);
            this.callback = callback;
            bgBlurredImage.enabled = false;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            bgBlurredImage.enabled = true;
        }

        private void OnYesClicked()
        {
            callback?.Invoke();
            Hide();
        }

        private void OnNoClicked()
        {
            Hide();
        }

    }
}