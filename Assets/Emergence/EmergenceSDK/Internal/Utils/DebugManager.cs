using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.Internal.Utils
{
    public class DebugManager : SingletonComponent<DebugManager>
    {
        [SerializeField] 
        private GameObject debugOverlayCanvas = null;
        [SerializeField]
        private TMP_Text buildInfoOutput = null;
        
        private string cachedBuildInfo = "";
        private bool isDebugOverlayActive = false;
        
        public void ToggleDebugOverlay()
        {
            isDebugOverlayActive = !isDebugOverlayActive;
            if (isDebugOverlayActive)
            {
                if (string.IsNullOrEmpty(cachedBuildInfo))
                {
                    cachedBuildInfo = BuildInfoGenerator.GetBuildInfo();
                    buildInfoOutput.text = cachedBuildInfo;
                }
                Debug.Log(cachedBuildInfo);
            }
            debugOverlayCanvas.SetActive(isDebugOverlayActive);
        }
    }
}