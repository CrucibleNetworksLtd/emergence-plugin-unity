using EmergenceSDK.Internal.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public abstract class DemoStation<T> : SingletonComponent<T> where T : SingletonComponent<T>
    {
        [FormerlySerializedAs("instructions")] 
        public GameObject instructionsGO;
        
        protected TextMeshProUGUI InstructionsText => instructionsText ??= instructionsGO.GetComponentInChildren<TextMeshProUGUI>();
        private TextMeshProUGUI instructionsText;

        protected string ActiveInstructions = "Press 'E' to activate";
        protected string InactiveInstructions = "Sign in using first station";

        protected bool isReady;
        
        protected bool HasBeenActivated() => Input.GetKeyDown(KeyCode.E) && instructionsGO.activeSelf;
    }
}