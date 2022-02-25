using UnityEngine;
//using WalletConnectSharp.Unity;

public class TestConnect : MonoBehaviour
{
    //public WalletConnect wc;
    public async void OnClickConnect()
    {
        //await wc.Connect();
    }

    public void OnError()
    {
        Debug.Log("ERROR!");
    }
}
