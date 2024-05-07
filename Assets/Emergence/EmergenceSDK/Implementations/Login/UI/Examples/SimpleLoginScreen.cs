using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login.Exceptions;
using EmergenceSDK.Implementations.Login.Types;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmergenceSDK.Implementations.Login.UI.Examples
{
    /// <summary>
    /// A minimalist implementation of a login widget to go with the <see cref="LoginManager"/><para/>
    /// Try it out on "SimpleLoginScreen.prefab" in the same folder as this script
    /// </summary>
    public class SimpleLoginScreen : MonoBehaviour
    {
        public LoginManager loginManager; // The LoginManager MonoBehaviour
        public LoginMode loginMode; // The chosen LoginMode
        public RawImage rawImage; // The RawImage for the QR code
        public TextMeshProUGUI countdownLabel; // The label for the QR code countdown
        public Button cancelButton; // The cancel button
        private void SetTimeRemainingText(LoginManager _, EmergenceQrCode qrCode) => countdownLabel.text = "Expires in: " + qrCode.TimeLeftInt.ToString("0");

        private void Awake()
        {
            // We bind all necessary events here

            // Call CancelLogin when hitting the cancel button
            // Since the LoginManager on this prefab is set to automatically cancel on disable, and given that this script simply disables the gameObject in the loginCancelledEvent,
            // It would have also been possible to simply cancel the login by disabling the gameObject and not handling the loginCancelledEvent event.
            cancelButton.onClick.AddListener(loginManager.CancelLogin);

            // Update the countdownLabel on each QR code tick */
            loginManager.qrCodeTickEvent.AddListener(SetTimeRemainingText);

            // Update UI when the login starts, setting QR as blank and adding a placeholder text to the countdownLabel */
            loginManager.loginStartedEvent.AddListener(HandleLoginStarted);

            // When login gets cancelled, disable the GameObject to hide it. Normally we would send the user to a previous screen or something similar to that. */
            loginManager.loginCancelledEvent.AddListener((_) => { gameObject.SetActive(false); });

            // When the login fails HandleLoginErrors is called and quickly disable and enable the Gameobject.
            // This is a hack to reset the UI upon failure given how the events have been setup.
            loginManager.loginFailedEvent.AddListener((manager, container) =>
            {
                HandleLoginErrors(manager, container);
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            });

            // Handle the login steps, we only care about LoginStep.QrCodeRequest succeeding in this minimalist implementation
            loginManager.loginStepUpdatedEvent.AddListener((_, loginStep, stepPhase) =>
            {
                if (stepPhase != StepPhase.Success) return; // Ignore steps that have just begun

                switch (loginStep)
                {
                    case LoginStep.QrCodeRequest:
                        var texture2D = loginManager.CurrentQrCode.Texture;
                        texture2D.filterMode = FilterMode.Point;
                        rawImage.texture = texture2D;
                        break;
                    case LoginStep.HandshakeRequest: // Nothing to do for these here
                    case LoginStep.AccessTokenRequest:
                    case LoginStep.FuturepassRequests:
                        break;
                    default: // For good measure in case of any unhandled steps
                        throw new ArgumentOutOfRangeException(nameof(loginStep), loginStep, null);
                }
            });

            // Handle successful login by simply hiding the gameObject and setting the first-login flag to true
            loginManager.loginSuccessfulEvent.AddListener((_, _) =>
            {
                LoginManager.SetFirstLoginFlag();
                gameObject.SetActive(false);
            });
        }

        private void Start()
        {
            // Unlock cursor on start for interacting with the prefab
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleLoginErrors(LoginManager _, LoginExceptionContainer exceptionContainer)
        {
            var e = exceptionContainer.Exception;
            switch (e)
            {
                // These are (in theory) all the expected and manageable exceptions
                case FuturepassRequestFailedException
                    or FuturepassInformationRequestFailedException
                    or TokenRequestFailedException
                    or HandshakeRequestFailedException
                    or QrCodeRequestFailedException:
                {
                    exceptionContainer.HandleException(); // Pretend we handled all of them
                    EmergenceLogger.LogWarning(e.GetType().FullName + ": " + e.Message); // Log exception
                    if (e.InnerException != null)
                    {
                        EmergenceLogger.LogWarning("\t" + e.InnerException.GetType().FullName + ": " + e.InnerException.Message); // Log inner exception
                    }

                    break;
                }
            }
        }

        private void HandleLoginStarted(LoginManager _)
        {
            rawImage.texture = null;
            countdownLabel.text = "Retrieving...";
        }

        private void OnEnable()
        {
            // Unlock cursor on enabling for interacting with the prefab
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Attempt starting a login attempt as soon as the widget is enabled, everytime
            // UniTask.Void executes as "fire and forget"
            UniTask.Void(async () =>
            {
                await loginManager.WaitUntilAvailable(); // Wait until the login is available
                await loginManager.StartLogin(loginMode); // Start login
            });
        }
    }
}