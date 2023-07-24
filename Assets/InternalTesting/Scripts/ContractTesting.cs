#if UNITY_EDITOR

using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.InternalTesting
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
            readContractMethodName = EditorGUILayout.TextField("Read Contract Method Name", readContractMethodName);
            
            if (GUILayout.Button("WriteContractMethod")) 
                WriteMethodPressed();
            writeContract = (DeployedSmartContract)EditorGUILayout.ObjectField("WriteContract", writeContract, typeof(DeployedSmartContract), true);
            writeContractMethodName = EditorGUILayout.TextField("Write Contract Method Name", writeContractMethodName);
        }
                
        private void ReadMethodPressed()
        {
            var contractInfo = new ContractInfo(readContract, readContractMethodName);
            EmergenceServices.GetService<IContractService>().ReadMethod(contractInfo,
                new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                (result) => EditorUtility.DisplayDialog("Read Method Result", "Result: " + result, "OK"),
                EmergenceLogger.LogError);
        }

        private void WriteMethodPressed()
        {
            var contractInfo = new ContractInfo(writeContract, writeContractMethodName);
            EmergenceServices.GetService<IContractService>().WriteMethod(contractInfo,
                "", "", "0", new string[] { },
                (response) => EditorUtility.DisplayDialog("Write Method Response", "Response: " + response, "OK"),
                EmergenceLogger.LogError);
        }
                
    }
}

#endif