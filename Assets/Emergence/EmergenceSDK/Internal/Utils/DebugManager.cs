using EmergenceSDK.Internal.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.Internal.Utils
{
    public class DebugManager : SingletonComponent<DebugManager>
    {
        /// <summary>
        /// The container for the Debug Overlay Canvas.
        /// </summary>
        [SerializeField] 
        private GameObject debugOverlayCanvas = null;
        
        /// <summary>
        /// The text element for displaying build info.
        /// </summary>
        [FormerlySerializedAs("DebugInfoOutput")] [FormerlySerializedAs("buildInfoOutput")] [SerializeField]
        private TMP_Text debugInfoOutput = null;
        
        private string cachedDebugInfo = "";
        private bool isDebugOverlayActive = false;
        private SessionService sessionService = null;
        private WalletService walletService = null;
        
        /// <summary>
        /// Toggles the visibility state of the Debug Overlay and prints to log.
        /// </summary>
        public void ToggleDebugOverlay()
        {
            isDebugOverlayActive = !isDebugOverlayActive;
            if (sessionService == null)
            {
                sessionService = EmergenceServiceProvider.GetService<SessionService>();
            }

            if (walletService == null)
            {
                walletService = EmergenceServiceProvider.GetService<WalletService>();
            }
            
            if (isDebugOverlayActive)
            { 
                string walletInfo = "";
                
                if (sessionService != null && walletService != null)
                {                
                    bool isWalletConnected = sessionService.IsLoggedIn;
                    walletInfo = string.Format("Wallet Connected: {0} WalletAddress: {1}", isWalletConnected.ToString(), isWalletConnected ? walletService.WalletAddress : "N/A");
                }
                else
                {
                    walletInfo = "Wallet Connected: False";
                }

                cachedDebugInfo = BuildInfoGenerator.GetBuildInfo() + walletInfo;
                debugInfoOutput.text = cachedDebugInfo;
                Debug.Log(cachedDebugInfo);
            }
            debugOverlayCanvas.SetActive(isDebugOverlayActive);
        }
        
        /// <summary>
        /// Called by UI Element, Copies Build info to clipboard.
        /// </summary>
        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = cachedDebugInfo;
            Debug.Log("Build info copied to clipboard.");
        }
    }
}