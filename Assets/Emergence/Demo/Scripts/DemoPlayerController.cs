using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DemoPlayerController : MonoBehaviour
{
    private DemoStationController currentDemoStation;

    private void Update()
    {
        if (currentDemoStation && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentDemoStation.invokeMethod.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        currentDemoStation = other.gameObject.GetComponent<DemoStationController>();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<DemoStationController>().Equals(currentDemoStation))
        {
            currentDemoStation = null;
        }
    }
}
