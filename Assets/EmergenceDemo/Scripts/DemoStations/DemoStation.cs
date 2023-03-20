using EmergenceSDK.Internal.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmergenceSDK.EmergenceDemo.DemoStations
{
    public abstract class DemoStation<T> : SingletonComponent<T> where T : SingletonComponent<T>
    {
        public GameObject instructions;
        protected bool HasBeenActivated() => Keyboard.current.eKey.wasPressedThisFrame && instructions.activeSelf;
    }
}