using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class EditPersonaScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Button backButton;
        public Pool avatarScrollItemsPool;
        public Transform avatarScrollRoot;
        public TextMeshProUGUI title;
        public Button createButton;
    }
}
