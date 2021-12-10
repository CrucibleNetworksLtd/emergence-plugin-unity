using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameObject viewPersonaScreen;

    [SerializeField]
    private GameObject editPersonaScreen;

    private enum ScreenStates
    {
        Welcome,
        LogIn,
        Dashboard,
        ViewPersona,
        EditPersona,
    }

    private ScreenStates state = ScreenStates.Welcome;

    private void Awake()
    {
        ChangeState(this.state);
    }

    private void ChangeState(ScreenStates state)
    {
        welcomeScreen.SetActive(false);
        logInScreen.SetActive(false);
        dashboardScreen.SetActive(false);
        viewPersonaScreen.SetActive(false);
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
            case ScreenStates.ViewPersona:
                viewPersonaScreen.SetActive(true);
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

    public void ShowViewPersona()
    {
        ChangeState(ScreenStates.ViewPersona);
    }

    public void ShowEditPersona()
    {
        ChangeState(ScreenStates.EditPersona);
    }
}
