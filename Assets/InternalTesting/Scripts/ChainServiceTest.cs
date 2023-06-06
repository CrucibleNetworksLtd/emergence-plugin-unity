#if UNITY_EDITOR
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types.Responses;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK
{
    public class ChainServiceTest : EditorWindow
    {
        [MenuItem("Window/Emergence Internal/Chain Service")]
        private static void OpenWindow()
        {
            ChainServiceTest window = GetWindow<ChainServiceTest>("Chain Service");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Test Chain Service");
            
            if (GUILayout.Button("GetTransactionStatus")) 
                GetTransactionStatusPressed();
            if (GUILayout.Button("GetHighestBlockNumber")) 
                GetHighestBlockNumberPressed();
        }

        private void GetTransactionStatusPressed()
        {
            var chainService = EmergenceServices.GetService<IChainService>();
            chainService.GetTransactionStatus<BaseResponse<string>>("0xb2eba081d2f21a55b4a2be0f73ce98233030051b1c59af64d50ea50dbd75f869", "https://goerli.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134",
                (status) => Debug.Log("Status: " + status), EmergenceLogger.LogError);
        }
        
        private void GetHighestBlockNumberPressed()
        {
            var chainService = EmergenceServices.GetService<IChainService>();
            chainService.GetHighestBlockNumber<BaseResponse<string>>("https://goerli.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134",
                (blockNumber) => Debug.Log("Block Number: " + blockNumber), EmergenceLogger.LogError);

        }
    }
}
#endif