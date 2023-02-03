using UnityEngine;
using EmergenceSDK;
using UnityEngine.InputSystem;

namespace EmergenceDemo
{
    public class DemoOpenOverlay : MonoBehaviour
    {

        [SerializeField] private GameObject instructions;

        private void Start()
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

        private void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame && instructions.activeSelf)
            {
                OpenOverlay();
            }
        }

        private void OpenOverlay()
        {
            EmergenceSingleton.Instance.OpenEmergenceUI();
        }
    }
}