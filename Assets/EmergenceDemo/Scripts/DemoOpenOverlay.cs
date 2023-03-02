using System;
using UnityEngine;
using EmergenceSDK;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Avatar = EmergenceSDK.Avatar;

namespace EmergenceDemo
{
    public class DemoOpenOverlay : MonoBehaviour
    {

        [SerializeField] private GameObject instructions;

        private void OnEnable() {
            // EventManager.StartListening(EmergenceEvents.AVATAR_LOADED, SwapAvatar);
            Services.OnCurrentPersonaUpdated += OnPersonaUpdated;
        }
        //
        // private void OnDisable() {
        //     throw new NotImplementedException();
        // }

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

        public void OnPersonaUpdated(Persona persona) {
            Debug.Log("Changing avatar");
            if (persona != null && !string.IsNullOrEmpty(persona.avatarId))
            {
                Services.Instance.AvatarById(persona.avatarId, (avatar =>
                {
                    DemoAvatarManager.Instance.SwapAvatars(avatar.meta.content[1].url);
                
                }), (message, code) =>
                {
                    Debug.LogError("Error fetching Avatar by id: " + message);
                });
            }
            else
            {
                DemoAvatarManager.Instance.SetDefaultAvatar();
            }
        }

        private void OpenOverlay() {
            EmergenceSingleton.Instance.OpenEmergenceUI();
        }
    }
}