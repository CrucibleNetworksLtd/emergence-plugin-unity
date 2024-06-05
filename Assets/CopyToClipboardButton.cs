using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK
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
