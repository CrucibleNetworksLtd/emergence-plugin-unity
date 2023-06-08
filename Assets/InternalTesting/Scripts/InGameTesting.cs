#if UNITY_EDITOR

using System.Collections.Generic;
using EmergenceSDK.EmergenceDemo.DemoStations;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.ScriptableObjects;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEditor;
using UnityEngine;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK
{
    public class InGameTesting : EditorWindow
    {
        private Texture2D qrcode;
        private bool displayQR = false;
        private bool displayFullTestUI = false;
        
        List<Avatar> avatars = new List<Avatar>();

        private DeployedSmartContract readContract;
        private string readContractMethodName;
        
        private DeployedSmartContract writeContract;
        private string writeContractMethodName;


        [MenuItem("Window/Emergence Internal/In Game Test Panel")]
        private static void OpenWindow()
        {
            InGameTesting window = GetWindow<InGameTesting>("In Game Test Panel");
            window.Show();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Hit play to test Emergence SDK");
                Cleanup();
                return;
            }

            if (!displayFullTestUI)
            {
                EditorGUILayout.LabelField("QR");
                if (GUILayout.Button("Generate QR"))
                {
                    EmergenceServices.GetService<ISessionService>().GetQRCode(OnGetQRCodeSuccess, EmergenceLogger.LogError);
                }
    
                if (displayQR)
                {
                    GUILayout.Label(qrcode);
                    return;
                }
                
                return;
            }
            
            EditorGUILayout.LabelField("Test Avatar Service");

            if (GUILayout.Button("TestAvatarsByOwner"))
            {
                EmergenceServices.GetService<IAvatarService>().AvatarsByOwner(EmergenceSingleton.Instance.GetCachedAddress(), (avatarsIn) => avatars = avatarsIn, EmergenceLogger.LogError);
            }
            EditorGUILayout.LabelField("Retrieved Avatars:");
            foreach (var avatar in avatars)
            {
                EditorGUILayout.LabelField("Avatar: " + avatar.meta.name);
                EditorGUILayout.LabelField("Contract: " + avatar.contractAddress);
            }
            
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Test Contract Service");
            
            if (GUILayout.Button("ReadContractMethod")) 
                ReadMethodPressed();
            readContract = (DeployedSmartContract)EditorGUILayout.ObjectField("ReadContract", readContract, typeof(DeployedSmartContract), true);
            readContractMethodName = EditorGUILayout.TextField("Read Contract Method Name", readContractMethodName);
            
            if (GUILayout.Button("WriteContractMethod")) 
                WriteMethodPressed();
            writeContract = (DeployedSmartContract)EditorGUILayout.ObjectField("WriteContract", writeContract, typeof(DeployedSmartContract), true);
            writeContractMethodName = EditorGUILayout.TextField("Write Contract Method Name", writeContractMethodName);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Test Inventory Service");
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField("Test Dynamic Metadata Service");
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Test Emergence Service");
            EditorGUILayout.Separator();


            EditorGUILayout.LabelField("Test Persona Service");
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Test Session Service");
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Test Wallet Service");
            EditorGUILayout.Separator();

        }

        private void ReadMethodPressed()
        {
            var contractInfo = new ContractInfo(readContract.contractAddress, readContractMethodName,
                readContract.chain.networkName, readContract.chain.DefaultNodeURL, readContract.contract.ABI);
            EmergenceServices.GetService<IContractService>().ReadMethod<ReadMethod.ContractResponse, string[]>(contractInfo,
                new string[] { EmergenceSingleton.Instance.GetCachedAddress() },
                (result) => EditorUtility.DisplayDialog("Read Method Result", "Result: " + result, "OK"),
                EmergenceLogger.LogError);
        }
        
        private void WriteMethodPressed()
        {
            var contractInfo = new ContractInfo(writeContract.contractAddress, writeContractMethodName,
                writeContract.chain.networkName, writeContract.chain.DefaultNodeURL, writeContract.contract.ABI);
            EmergenceServices.GetService<IContractService>().WriteMethod<BaseResponse<string>, string[]>(contractInfo,
                "", "", "0", new string[] { },
                (response) => EditorUtility.DisplayDialog("Write Method Response", "Response: " + response, "OK"),
                EmergenceLogger.LogError);
        }
        
        private void Cleanup()
        {
            displayQR = false;
            displayFullTestUI = false;
            
            avatars.Clear();
        }

        private void OnGetQRCodeSuccess(Texture2D qrcodeIn)
        {
            qrcode = qrcodeIn;
            displayQR = true;
            EmergenceServices.GetService<IWalletService>().Handshake((walletAddress) =>
                {
                    EmergenceLogger.LogInfo("Hand shook with wallet: " + walletAddress);
                    displayQR = false;
                    displayFullTestUI = true;
                    Repaint();
                },EmergenceLogger.LogError);
            Repaint();
        }
    }
}

#endif