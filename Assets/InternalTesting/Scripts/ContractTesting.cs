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
            
#if C9571AaF9EbCa8C08EC37D962310d0Ab9F8A5Dd2
            EditorGUILayout.LabelField("Generated Counter Contract");
            if (GUILayout.Button("Read Counter"))
                ReadCounterPressed();
            
            if (GUILayout.Button("Write Counter"))
                WriteCounterPressed();
#endif
        }
        
        
#if C9571AaF9EbCa8C08EC37D962310d0Ab9F8A5Dd2
        private void ReadCounterPressed()
        {
            throw new System.NotImplementedException();
        }
        
        private void WriteCounterPressed()
        {
            throw new System.NotImplementedException();
        }
#endif
        private void ReadMethodPressed()
        {
            var contractInfo = new ContractInfo(readContract.contractAddress, readContractMethodName,
                readContract.chain.networkName, readContract.chain.DefaultNodeURL, readContract.contract.ABI);
            EmergenceServices.GetService<IContractService>().ReadMethod<ReadMethod.ContractResponse>(contractInfo,
                EmergenceSingleton.Instance.GetCachedAddress(),
                (result) => EditorUtility.DisplayDialog("Read Method Result", "Result: " + result, "OK"),
                EmergenceLogger.LogError);
        }

        private void WriteMethodPressed()
        {
            var contractInfo = new ContractInfo(writeContract.contractAddress, writeContractMethodName,
                writeContract.chain.networkName, writeContract.chain.DefaultNodeURL, writeContract.contract.ABI);
            EmergenceServices.GetService<IContractService>().WriteMethod<BaseResponse<string>>(contractInfo,
                "", "", "0", "",
                (response) => EditorUtility.DisplayDialog("Write Method Response", "Response: " + response, "OK"),
                EmergenceLogger.LogError);
        }
                
    }
}

#endif