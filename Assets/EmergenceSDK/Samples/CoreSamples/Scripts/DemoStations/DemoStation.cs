using EmergenceSDK.Runtime.Internal.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public abstract class DemoStation<T> : SingletonComponent<T> where T : SingletonComponent<T>
    {
        public GameObject instructionsGO;

        protected TextMeshProUGUI InstructionsText => instructionsText ??= instructionsGO.GetComponentInChildren<TextMeshProUGUI>();
        private TextMeshProUGUI instructionsText;

        protected string ActiveInstructions = "Press 'E' to activate";
        protected string InactiveInstructions = "Sign in using first station";

        protected bool isReady;
        
        protected bool HasBeenActivated()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current.eKey.wasPressedThisFrame && instructionsGO.activeSelf;
#else
            Debug.LogWarning("These samples are dependent on the new Input System package. Please enable the new Input System.");
            return false;
#endif
        }
    }
}