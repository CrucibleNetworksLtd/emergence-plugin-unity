#if UNITY_EDITOR
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class ChainTesting : BaseTestWindow
    {
        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            
            EditorGUILayout.LabelField("Test Chain Service");
            
            if (GUILayout.Button("GetTransactionStatus")) 
                GetTransactionStatusPressed();
            if (GUILayout.Button("GetHighestBlockNumber")) 
                GetHighestBlockNumberPressed();
            
            EditorGUILayout.Separator();
        }

        private void GetTransactionStatusPressed()
        {
            var chainService = EmergenceServiceProvider.GetService<IChainService>();
            chainService.GetTransactionStatus("0xb2eba081d2f21a55b4a2be0f73ce98233030051b1c59af64d50ea50dbd75f869", "https://goerli.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134",
                (status) => Debug.Log("Status: " + status.transaction), EmergenceLogger.LogError);
        }
        
        private void GetHighestBlockNumberPressed()
        {
            var chainService = EmergenceServiceProvider.GetService<IChainService>();
            chainService.GetHighestBlockNumber("https://goerli.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134",
                (blockNumber) => Debug.Log("Block Number: " + blockNumber), EmergenceLogger.LogError);
        }
    }
}
#endif