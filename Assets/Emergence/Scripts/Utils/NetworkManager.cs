using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
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
            UnityEngine.Debug.Log("Running Emergence Server");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Server error: " + e.Message);
        }
    }
}
