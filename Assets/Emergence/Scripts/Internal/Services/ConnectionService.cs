using System.Collections;
using EmergenceSDK;
using EmergenceSDK.Emergence.Scripts.Emergence.Services;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionService : MonoBehaviour, IConnectionService
{
    public void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
    {
        StartCoroutine(CoroutineIsConnected(success, errorCallback));
    }

    private IEnumerator CoroutineIsConnected(IsConnectedSuccess success, ErrorCallback errorCallback)
    {
        Debug.Log("CoroutineIsConnected request started");

        // string url = EmergenceSingleton.Instance.Configuration.APIBase + "isConnected";
        string url = EmergenceSingleton.Instance.Configuration.APIBase + "isConnected";
        Debug.Log("url: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("deviceId", EmergenceSingleton.Instance.CurrentDeviceId);
            yield return request.SendWebRequest();
            Services.PrintRequestResult("IsConnected", request);
            if (Services.ProcessRequest<IsConnectedResponse>(request, errorCallback, out var response))
            {
                success?.Invoke(response.isConnected);
            }
        }
    }
}
