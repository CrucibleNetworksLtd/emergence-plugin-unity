#if UNITY_EDITOR

using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class ContractTesting : BaseTestWindow
    {

        private DeployedSmartContract readContract;
        private string readContractMethodName;
        
        private DeployedSmartContract writeContract;
        private string writeContractMethodName;
        
        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            needsCleanUp = true;
            
            EditorGUILayout.LabelField("Test Contract Service");
            
            if (GUILayout.Button("ReadContractMethod")) 
                ReadMethodPressed();
            readContract = (DeployedSmartContract)EditorGUILayout.ObjectField("ReadContract", readContract, typeof(DeployedSmartContract), true);
            readContractMethodName = EditorGUILayout.TextField("Read Contract Method PersonaName", readContractMethodName);
            
            if (GUILayout.Button("WriteContractMethod")) 
                WriteMethodPressed();
            writeContract = (DeployedSmartContract)EditorGUILayout.ObjectField("WriteContract", writeContract, typeof(DeployedSmartContract), true);
            writeContractMethodName = EditorGUILayout.TextField("Write Contract Method PersonaName", writeContractMethodName);
        }
                
        private void ReadMethodPressed()
        {
            var contractInfo = new ContractInfo(readContract, readContractMethodName);
            EmergenceServiceProvider.GetService<IContractService>().ReadMethod(contractInfo,
                new string[] { EmergenceServiceProvider.GetService<IWalletService>().WalletAddress },
                (result) => EditorUtility.DisplayDialog("Read Method Result", "Result: " + result, "OK"),
                EmergenceLogger.LogError);
        }

        private void WriteMethodPressed()
        {
            var contractInfo = new ContractInfo(writeContract, writeContractMethodName);
            EmergenceServiceProvider.GetService<IContractService>().WriteMethod(contractInfo, "0", new string[] { },
                (response) => EditorUtility.DisplayDialog("Write Method Response", "Response: " + response, "OK"),
                EmergenceLogger.LogError);
        }
                
    }
}

#endif