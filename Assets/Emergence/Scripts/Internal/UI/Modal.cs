using TMPro;
using UnityEngine;

namespace EmergenceSDK.Internal.UI
{
    public class Modal : MonoBehaviour
    {
        public TextMeshProUGUI label;

        public static Modal Instance;

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show(string message)
        {
            label.text = message;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}