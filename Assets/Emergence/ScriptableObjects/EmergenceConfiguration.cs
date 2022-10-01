using EmergenceSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = EmergenceEditor.basePath + "Configuration", order = 1)]
public class EmergenceConfiguration : ScriptableObject
{
    public string APIBase = "http://localhost/";
    public string DefaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";
    public string databaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";
    public string CustomEmergenceServerLocation = "C:\\Dev\\emergence-evm-server\\bin\\Debug\\net5.0\\EmergenceEVMLocalServer.exe";
    public string CustomEmergenceServerURL = "http://localhost:50733/";
}