using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Runtime
{
    public class CopyToClipboardButton : MonoBehaviour
    {
        public TMP_InputField sourceInput;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => GUIUtility.systemCopyBuffer = sourceInput.text);
        }
    }
}
