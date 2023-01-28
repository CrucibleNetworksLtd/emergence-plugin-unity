using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using static EmergenceSDK.Services;
using Cysharp.Threading.Tasks;

namespace EmergenceSDK
{
    public class CloudEmergenceServer : SingletonComponent<LocalEmergenceServer>
    {
        private string EVMServerURL = "http://evm.openmeta.xyz/";

        public async void InitializeEVMServer()
        {
            UnityWebRequest request  = UnityWebRequest.Get(EVMServerURL + "GetQRCode");
                
                try
                {
                    // return (await request.SendWebRequest()).downloadHandler.text;
                    string devideId = (await request.SendWebRequest()).GetResponseHeader("deviceId");
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                   
                }
            
        }
    }
}
