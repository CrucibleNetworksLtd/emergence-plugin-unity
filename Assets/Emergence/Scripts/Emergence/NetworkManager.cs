using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager
{

    public readonly string APIBase = "http://localhost:50733/api/";
    public readonly string DatabaseAPIPublic = "https://pfy3t4mqjb.execute-api.us-east-1.amazonaws.com/staging/";
    public readonly string DatabaseAPIPrivate = "https://57l0bi6g53.execute-api.us-east-1.amazonaws.com/staging/";
    public readonly string defaultNodeURL = "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134";


    public IEnumerator PingServer(string url)
    {
        while (true)
        {
            WaitForSeconds f = new WaitForSeconds(0.05f);
            Ping p = new Ping(url);

            while (!p.isDone)
            {
                yield return f;
            }

        }
    }

    public static void StartServer()
    {
        try
        {
            System.Diagnostics.Process.Start("run-server.bat");
            Debug.Log("Running Emergence Server");
        }
        catch (Exception e)
        {
            Debug.Log("Server error: " + e.Message);
        }
    }
}
