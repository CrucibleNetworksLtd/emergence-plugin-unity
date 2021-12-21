using TMPro;
using UnityEngine;

namespace Emergence
{
    public class Modal : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public CanvasGroup cg;


        public static Modal Instance;

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show(string message)
        {
            cg.alpha = 0.5f;
            label.text = message;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            cg.alpha = 1.0f;
            gameObject.SetActive(false);
        }
    }
}