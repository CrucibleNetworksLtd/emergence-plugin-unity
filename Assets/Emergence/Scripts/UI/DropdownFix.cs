using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergenceSDK
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