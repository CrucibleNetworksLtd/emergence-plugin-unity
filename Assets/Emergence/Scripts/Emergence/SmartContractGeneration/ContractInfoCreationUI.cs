#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;
using ABIToDotNet;
using EmergenceSDK.Internal.Utils;

public class ContractInfoCreationUI : EditorWindow
{
    private string address;
    private string methodName;
    private string network;
    private string abi;

    [MenuItem("Window/Contract Info Creation")]
    public static void ShowWindow()
    {
        GetWindow<ContractInfoCreationUI>("Contract Info Creation");
    }

    private void OnGUI()
    {
        GUILayout.Label("Contract Info Creation", EditorStyles.boldLabel);

        address = EditorGUILayout.TextField("Contract Address", address);
        methodName = EditorGUILayout.TextField("Method Name", methodName);
        network = EditorGUILayout.TextField("Network", network);
        abi = EditorGUILayout.TextField("ABI", abi);

        if (GUILayout.Button("Create Contract Info"))
        {
            if (ValidateInput())
            {
                CreateContractInfo();
            }
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrEmpty(address))
        {
            EditorUtility.DisplayDialog("Input Error", "Contract address cannot be empty.", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(methodName))
        {
            EditorUtility.DisplayDialog("Input Error", "Method name cannot be empty.", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(network))
        {
            EditorUtility.DisplayDialog("Input Error", "Network cannot be empty.", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(abi))
        {
            EditorUtility.DisplayDialog("Input Error", "ABI cannot be empty.", "OK");
            return false;
        }

        return true;
    }

    private void CreateContractInfo()
    {
        ContractInfo newContractInfo = new ContractInfo(address, methodName, network, "", abi);

        // Get the path of the current script
        string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));

        // Get the directory path of the current script
        string directoryPath = Path.GetDirectoryName(scriptPath);
        string generatedPath = Path.Combine(directoryPath, "Generated");
        // Create the directory for the network
        string networkPath = Path.Combine(generatedPath, network);
        if (!Directory.Exists(networkPath))
        {
            Directory.CreateDirectory(networkPath);
        }

        // Create a new file with the contract info
        string filePath = Path.Combine(networkPath, $"{methodName}.cs");
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, new ABIToCSharp(newContractInfo).CSharpClass);
            Debug.Log("Created ContractInfo file: " + filePath);
        }
        else
        {
            Debug.Log("ContractInfo file already exists: " + filePath);
        }
    }
}

#endif
