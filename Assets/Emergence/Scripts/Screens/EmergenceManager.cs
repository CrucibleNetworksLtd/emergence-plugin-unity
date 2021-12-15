using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Emergence
{
    public class EmergenceManager : MonoBehaviour
    {
        [Header("Screen references")]
        [SerializeField]
        private GameObject welcomeScreen;

        [SerializeField]
        private GameObject logInScreen;

        [SerializeField]
        private GameObject dashboardScreen;

        [SerializeField]
        private GameObject editPersonaScreen;

        [Header("UI Reference")]
        public Button escButton;

        private enum ScreenStates
        {
            Welcome,
            LogIn,
            Dashboard,
            EditPersona,
        }

        private ScreenStates state = ScreenStates.Welcome;

        public static EmergenceManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            ChangeState(this.state);
            escButton.onClick.AddListener(OnEscButtonPressed);
        }

        public delegate void ButtonEsc();

        public static event ButtonEsc OnButtonEsc;

        private void OnEscButtonPressed()
        {
            OnButtonEsc?.Invoke();
        }

        private void ChangeState(ScreenStates state)
        {
            welcomeScreen.SetActive(false);
            logInScreen.SetActive(false);
            dashboardScreen.SetActive(false);
            editPersonaScreen.SetActive(false);

            this.state = state;

            switch (state)
            {
                case ScreenStates.Welcome:
                    welcomeScreen.SetActive(true);
                    break;
                case ScreenStates.LogIn:
                    logInScreen.SetActive(true);
                    break;
                case ScreenStates.Dashboard:
                    dashboardScreen.SetActive(true);
                    break;
                case ScreenStates.EditPersona:
                    editPersonaScreen.SetActive(true);
                    break;
            }
        }

        public void ShowWelcome()
        {
            ChangeState(ScreenStates.Welcome);
        }

        public void ShowLogIn()
        {
            ChangeState(ScreenStates.LogIn);
        }

        public void ShowDashboard()
        {
            ChangeState(ScreenStates.Dashboard);
        }

        public void ShowEditPersona()
        {
            ChangeState(ScreenStates.EditPersona);
        }
    }
}