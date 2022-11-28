using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EmergenceSDK;

public class DemoStationController : MonoBehaviour
{
    [SerializeField] private GameObject instructions;

    public UnityEvent invokeMethod;

    void Start()
    {
        instructions.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        instructions.SetActive(true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        instructions.SetActive(false);
    }

    public void OpenOverlay()
    {
        EmergenceSingleton.Instance.OpenEmergenceUI();
    }
}
