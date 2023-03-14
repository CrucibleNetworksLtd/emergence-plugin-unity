using UnityEngine;

namespace EmergenceSDK.Internal.UI
{
    public class DropdownFix : MonoBehaviour
    {
        // public string _SortingLayerName = "Default";

        void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
                canvas.overrideSorting = false;
                // canvas.sortingLayerName = _SortingLayerName;
        }
    }
}