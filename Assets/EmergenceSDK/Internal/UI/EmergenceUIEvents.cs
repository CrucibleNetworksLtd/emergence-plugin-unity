using System;
using UnityEngine.Events;

namespace EmergenceSDK.Internal.UI
{
    public class EmergenceUIEvents
    {
        [Serializable]
        public class EmergenceUIStateChanged : UnityEvent<bool>
        {
        }

        [Serializable]
        public class EmergenceUIOpened : UnityEvent
        {
        }

        [Serializable]
        public class EmergenceUIClosed : UnityEvent
        {
        }
    }
}