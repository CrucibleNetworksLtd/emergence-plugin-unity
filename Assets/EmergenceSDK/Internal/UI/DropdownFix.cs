using UnityEngine;

namespace EmergenceSDK.Internal.UI
{
    public class DropdownFix : MonoBehaviour
    {
        void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.overrideSorting = false;
        }
    }
}