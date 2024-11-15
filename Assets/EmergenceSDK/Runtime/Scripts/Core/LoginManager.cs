using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Events.Login;
using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Futureverse.Internal.Services;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Exceptions.Login;
using EmergenceSDK.Runtime.Types.Responses;
using UniJSON;
using UnityEngine;
using UnityEngine.Events;

namespace EmergenceSDK.Runtime
{
    /// <summary>
    /// This is designed to merely be a UI LoginManager, exposing the login flow for usage with custom UIs.<para/>
    /// Performing any business logic with this class is not recommended, for connection and disconnection logic
    /// you should take a look at <see cref="ISessionService"/>, specifically <see cref="ISessionService.OnSessionConnected"/>
    /// and <see cref="ISessionService.OnSessionDisconnected"/>
    /// <remarks>Note that while it is possible to configure whether the <see cref="LoginManager"/> should automatically cancel login attempts when getting disabled,
    /// they will always be automatically cancelled when it gets destroyed.</remarks>
    /// <seealso cref="ISessionService"/>
    /// </summary>
    public sealed class LoginManager : MonoBehaviour
    {
        /// <summary>
        /// The timeout in seconds that will be used for each QR code shown to the user.
        /// </summary>
        internal float qrCodeTimeout => EmergenceSingleton.Instance.QrCodeTimeout;
        /// <summary>
        /// If true, when the MonoBehaviour gets disabled the login will automatically be canceled. 
        /// </summary>
        public bool cancelLoginsUponDisabling = true;
        /// <summary>
        /// Whether the <see cref="LoginManager"/> is busy with another login session.
        /// </summary>
        public bool IsBusy { get; private set; }
        /// <summary>
        /// The currently active instance of <see cref="EmergenceQrCode"/> for the login session.
        /// </summary>
        public EmergenceQrCode CurrentQrCode { get; internal set; }
        /// <summary>
        /// Called after <see cref="StartLogin"/> is called, if <see cref="IsBusy"/> is false.
        /// <seealso cref="LoginStartedEvent"/>
        /// </summary>
        public LoginStartedEvent loginStartedEvent;
        /// <summary>
        /// Called after <see cref="CancelLogin"/> is called, if <see cref="IsBusy"/> is true.
        /// <seealso cref="LoginCancelledEvent"/>
        /// </summary>
        public LoginCancelledEvent loginCancelledEvent;
        /// <summary>
        /// Called when an error occurs during login.
        /// <seealso cref="LoginFailedEvent"/>
        /// </summary>
        public LoginFailedEvent loginFailedEvent;
        /// <summary>
        /// Called after a successful login.<para/>
        /// Do not use this event for business logic, as right after it is called, <see cref="ISessionService.OnSessionConnected"/> will also be called. That is where any business logic should go.
        /// <seealso cref="LoginSuccessfulEvent"/>
        /// </summary>
        public LoginSuccessfulEvent loginSuccessfulEvent;
        /// <summary>
        /// Called at each login step, useful for updating the UI. It will be called twice, when the step begins and after it succeeds.
        /// <remarks>There isn't a built-in way to keep track of the current login step, if you need this information you should store it yourself.</remarks>
        /// <seealso cref="LoginStepUpdatedEvent"/>
        /// </summary>
        public LoginStepUpdatedEvent loginStepUpdatedEvent;
        /// <summary>
        /// Called when the login process ends, always, no matter the reason.
        /// <seealso cref="LoginEndedEvent"/>
        /// 
        /// </summary>
        public LoginEndedEvent loginEndedEvent;
        /// <summary>
        /// Ticks every second after the QR code is retrieved. It begins ticking exactly after the <see cref="LoginStep.QrCodeRequest"/> <see cref="LoginStep"/> succeeds.
        /// <remarks>This keeps getting called even after the handshake is completed and is no longer needed, it only stops right before <see cref="loginEndedEvent"/> gets invoked.<para/>
        /// If you wish to stop this timer you should simply unbind from it when the handshake step is completed,
        /// listening to the <see cref="loginStepUpdatedEvent"/> event. Alternatively you can use your own event.</remarks>
        /// <seealso cref="QrCodeTickEvent"/>
        /// </summary>
        public QrCodeTickEvent qrCodeTickEvent;
        
        private CancellationTokenSource cts;
        private CancellationToken ct;

        private Action triggerDisconnectEvents;

        /// <summary>
        /// Set the first-login flag to true
        /// </summary>
        public static void SetFirstLoginFlag()
        {
            PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 1);
        }

        /// <summary>
        /// Set the first-login flag to false
        /// </summary>
        public static void ResetFirstLoginFlag()
        {
            PlayerPrefs.SetInt(StaticConfig.HasLoggedInOnceKey, 0);
        }

        /// <summary>
        /// Check whether the first-login flag is present and set to something different than 0
        /// </summary>
        /// <returns></returns>
        public static bool GetFirstLoginFlag()
        {
            return PlayerPrefs.GetInt(StaticConfig.HasLoggedInOnceKey, 0) != 0;
        }
        
        /// <summary>
        /// Starts the login attempt.
        /// <remarks>This won't start if another attempt is ongoing, you can check beforehand using <see cref="IsBusy"/> or by awaiting <see cref="WaitUntilAvailable"/> first</remarks>
        /// </summary>
        /// <param name="loginSettings">What settings to use for the login attempt, see <see cref="LoginSettings"/> for more details.</param>
        public async UniTask StartLogin(LoginSettings loginSettings)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var sessionServiceInternal = EmergenceServiceProvider.GetService<ISessionServiceInternal>();
                var walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
                var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
                var custodialLoginService = EmergenceServiceProvider.GetService<ICustodialLoginService>();

                cts = new CancellationTokenSource();
                ct = cts.Token;

                InvokeEventAndCheckCancellationToken(loginStartedEvent, this, ct);

                await HandleQrCodeRequest(loginSettings, sessionServiceInternal);
                await HandleCustodialRequests(loginSettings, custodialLoginService, sessionServiceInternal);
                await HandleHandshakeRequest(walletServiceInternal, loginSettings);
                await HandleAccessTokenRequest(loginSettings, sessionServiceInternal);
                await HandleFuturepassRequests(loginSettings, futureverseService);

                sessionServiceInternal.RunConnectionEvents(loginSettings);
                triggerDisconnectEvents = sessionServiceInternal.RunDisconnectionEvents;
                await UniTask.SwitchToMainThread();
                
                loginSuccessfulEvent.Invoke(this, ((IWalletService)walletServiceInternal).ChecksummedWalletAddress);
            }
            catch (OperationCanceledException)
            {
                loginCancelledEvent.Invoke(this);
            }
            catch (Exception e)
            {
                InvokeLoginFailedEvent(e);
            }
            finally
            {
                CurrentQrCode?.StopTicking();
                CurrentQrCode = null;
                loginEndedEvent.Invoke(this);
                IsBusy = false;
            }
        }

        /// <summary>
        /// This cancels the current login attempt, and fires the appropriate events.
        /// <remarks>Only works if there is a login attempt currently ongoing, you can check that with <see cref="IsBusy"/></remarks>
        /// </summary>
        public void CancelLogin()
        {
            if (!IsBusy) return;
            cts?.Cancel();
        }

        public async UniTask Disconnect()
        {
            await WaitUntilAvailable();
            triggerDisconnectEvents?.Invoke();
        }

        /// <summary>
        /// Waits until the <see cref="IsBusy"/> property is false.
        /// <remarks>You can provide a cancellation token to customize the wait time.</remarks>
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> for manually cancelling this.</param>
        /// <returns></returns>
        public UniTask WaitUntilAvailable(CancellationToken ct = default)
        {
            return UniTask.WaitUntil(() => !IsBusy, cancellationToken: ct);
        }

        /// <summary>
        /// Helper method for clearing all the event listeners if needed.
        /// </summary>
        public void RemoveAllListeners()
        {
            loginStartedEvent.RemoveAllListeners();
            loginCancelledEvent.RemoveAllListeners();
            loginFailedEvent.RemoveAllListeners();
            loginSuccessfulEvent.RemoveAllListeners();
            loginStepUpdatedEvent.RemoveAllListeners();
            loginEndedEvent.RemoveAllListeners();
            qrCodeTickEvent.RemoveAllListeners();
        }
        
        private void OnDestroy()
        {
            CancelLogin();
        }

        private void OnDisable()
        {
            if (cancelLoginsUponDisabling)
            {
                CancelLogin();
            }
        }

        private void InvokeLoginFailedEvent(Exception e)
        {
            var loginExceptionContainer = new LoginExceptionContainer(e);
            loginFailedEvent.Invoke(this, loginExceptionContainer);
            loginExceptionContainer.ThrowIfUnhandled();
        }

        private async UniTask HandleFuturepassRequests(LoginSettings loginSettings, IFutureverseService futureverseService)
        {
            if (loginSettings.HasFlag(LoginSettings.EnableFuturepass) && !loginSettings.HasFlag(LoginSettings.EnableCustodialLogin))
            {
                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.FuturepassRequests, StepPhase.Start, ct);
                ServiceResponse<LinkedFuturepassResponse> passResponse;
                try
                {
                    passResponse = await futureverseService.GetLinkedFuturepassAsync();
                    ct.ThrowIfCancellationRequested();
                    if (!passResponse.Successful)
                    {
                        throw new FuturepassRequestFailedException("Request was not successful", passResponse);
                    }
                    var ownedFuturePass = passResponse.Result1.ownedFuturepass;
                    await ProcessFuturePassResponse(futureverseService, ownedFuturePass);
                }
                catch (Exception e) when (e is not OperationCanceledException and not FuturepassRequestFailedException)
                {
                    throw new FuturepassRequestFailedException("An exception caused the request to fail", null, e);
                }
            }
        }
        
        /// <summary>
        /// Handles Custodial web login, built on the archetype provided by Futureverse to utilise their web login flow.
        /// </summary>
        /// <param name="loginSettings">The login settings for this session, used to determine whether to perform Custodial functionality</param>
        /// <param name="custodialLoginService">The Emergence Service for handling Custodial Logins</param>
        /// <param name="sessionServiceInternal">The emergence Service for handling the session</param>
        private async UniTask HandleCustodialRequests(LoginSettings loginSettings, ICustodialLoginService custodialLoginService , ISessionServiceInternal sessionServiceInternal)
        {
            if (loginSettings.HasFlag(LoginSettings.EnableCustodialLogin))
            {
                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.CustodialRequests, StepPhase.Start, ct);

                try
                {
                    // We need to call this to generate a Device ID through the backend.
                    await sessionServiceInternal.GetQrCodeAsync(ct);
                    // Trigger the custodial login service to handle the login flow.
                    await custodialLoginService.StartCustodialLoginAsync(CacheCustodialResponse,ct);
                }
                catch (Exception ex)
                {
                    // Handle errors during custodial login
                    InvokeLoginFailedEvent(ex);
                    throw new TokenRequestFailedException("Custodial login failed.", null, ex);
                }
            }
        }

        /// <summary>
        /// Callback method that initiates the exchange of a custodial bearer token for auth in other services.
        /// </summary>
        /// <param name="response"> The custodial Access token response sent to our local HTTP listener</param>
        /// <param name="ct"> The cancellation Token</param>
        private async UniTask CacheCustodialResponse(CustodialAccessTokenResponse response, CancellationToken ct)
        {
            try
            {
                var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
                // Mark the custodial login step as successful
                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.CustodialRequests, StepPhase.Success, ct);
                // We need to prepend the chain data to our futurepass, so we use the chain ID to determine the chain name
                string chainName = response.DecodedToken.ChainId switch
                {
                    7668 => "root",
                    7672 => "root",
                    _ => throw new ArgumentOutOfRangeException(response.DecodedToken.ChainId.ToString(), "Unknown chainID")
                };
                futureverseService.SetCustodialStatus(true);
                ct.ThrowIfCancellationRequested();
                await ProcessFuturePassResponse(futureverseService, $"{response.DecodedToken.ChainId}:{chainName}:{response.DecodedToken.Futurepass}");
            }
            catch (Exception ex)
            {
                InvokeLoginFailedEvent(ex);
            }
        }

        /// <summary>
        /// Processes a FV login response to get user information wth an associated Future Pass.
        /// </summary>
        /// <param name="futureverseService"></param>
        /// <param name="ownedFuturePass"></param>
        /// <exception cref="FuturepassInformationRequestFailedException"></exception>
        private async UniTask ProcessFuturePassResponse(IFutureverseService futureverseService, string ownedFuturePass)
        {
            try
            {
                var passInformationResponse = await futureverseService.GetFuturepassInformationAsync(ownedFuturePass);
                ct.ThrowIfCancellationRequested();
                if (!passInformationResponse.Successful || passInformationResponse.Result1 == null)
                {
                    if (passInformationResponse.Successful && passInformationResponse.Result1 == null)
                    {
                        var exception = new NullReferenceException(nameof(passInformationResponse) + '.' + nameof(passInformationResponse.Result1) + "is null");
                        throw new FuturepassInformationRequestFailedException("Request was successful but result is null!", passInformationResponse, exception);
                    }
                        
                    throw new FuturepassInformationRequestFailedException("Request was not successful", passInformationResponse);
                }
                ((IFutureverseServiceInternal)futureverseService).CurrentFuturepassInformation = passInformationResponse.Result1;
            }
            catch (Exception e) when (e is not OperationCanceledException and not FuturepassInformationRequestFailedException)
            {
                throw new FuturepassInformationRequestFailedException("An exception caused the request to fail", null, e);
            }

            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.FuturepassRequests, StepPhase.Success, ct);
        }

        private async UniTask HandleAccessTokenRequest(LoginSettings loginSettings, ISessionServiceInternal sessionServiceInternal)
        {
            if (!loginSettings.HasFlag(LoginSettings.DisableEmergenceAccessToken))
            {
                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.AccessTokenRequest, StepPhase.Start, ct);

                try
                {
                    if (loginSettings.HasFlag(LoginSettings.EnableCustodialLogin))
                    {
                        var tokenResponse = await sessionServiceInternal.GetCustodialAccessToken(ct);
                        if (!tokenResponse.Successful)
                        {
                            throw new TokenRequestFailedException("Request was not successful", tokenResponse);
                        }
                    }
                    else
                    {
                        var tokenResponse = await sessionServiceInternal.GetAccessTokenAsync();
                        if (!tokenResponse.Successful)
                        {
                            throw new TokenRequestFailedException("Request was not successful", tokenResponse);
                        }
                    }
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception e) when (e is not OperationCanceledException and not TokenRequestFailedException)
                {
                    throw new TokenRequestFailedException("An exception caused the request to fail", null, e);
                }

                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.AccessTokenRequest, StepPhase.Success, ct);
            }
        }

        private async UniTask HandleHandshakeRequest(IWalletServiceInternal walletServiceInternal, LoginSettings loginSettings)
        {
            if (loginSettings.HasFlag(LoginSettings.EnableCustodialLogin))
                return;
            
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.HandshakeRequest, StepPhase.Start, ct);
            try
            {
                var handshakeResponse = await walletServiceInternal.HandshakeAsync(ct: ct, timeout: qrCodeTimeout * 1000);
                ct.ThrowIfCancellationRequested();
                if (!handshakeResponse.Successful)
                {
                    throw new HandshakeRequestFailedException("Request was not successful", handshakeResponse);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException and not HandshakeRequestFailedException)
            {
                throw new HandshakeRequestFailedException("An exception caused the request to fail", null, e);
            }
            
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.HandshakeRequest, StepPhase.Success, ct);
        }

        private async UniTask HandleQrCodeRequest(LoginSettings loginSettings, ISessionServiceInternal sessionServiceInternal)
        {
            if (loginSettings.HasFlag(LoginSettings.EnableCustodialLogin))
            {
                return;
            }
            
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.QrCodeRequest, StepPhase.Start, ct);
                
            try
            {
                var qrCodeResponse = await sessionServiceInternal.GetQrCodeAsync(ct);
                ct.ThrowIfCancellationRequested();
                if (!qrCodeResponse.Successful)
                {
                    throw new QrCodeRequestFailedException("Request was not successful", qrCodeResponse);
                }
                CurrentQrCode = new EmergenceQrCode(this, qrCodeResponse.Result1, qrCodeResponse.Result2, EmergenceSingleton.Instance.CurrentDeviceId);
            }
            catch (Exception e) when (e is not OperationCanceledException and not QrCodeRequestFailedException)
            {
                throw new QrCodeRequestFailedException("An exception caused the request to fail", null, e);
            }
            
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.QrCodeRequest, StepPhase.Success, ct);
        }
        
        private void InvokeEventAndCheckCancellationToken(UnityEvent unityEvent, CancellationToken ct)
        {
            unityEvent.Invoke();
            ct.ThrowIfCancellationRequested();
        }

        private void InvokeEventAndCheckCancellationToken<T0>(UnityEvent<T0> unityEvent, T0 arg0, CancellationToken ct)
        {
            unityEvent.Invoke(arg0);
            ct.ThrowIfCancellationRequested();
        }

        private void InvokeEventAndCheckCancellationToken<T0, T1>(UnityEvent<T0, T1> unityEvent, T0 arg0, T1 arg1, CancellationToken ct)
        {
            unityEvent.Invoke(arg0, arg1);
            ct.ThrowIfCancellationRequested();
        }

        private void InvokeEventAndCheckCancellationToken<T0, T1, T2>(UnityEvent<T0, T1, T2> unityEvent, T0 arg0, T1 arg1, T2 arg2, CancellationToken ct)
        {
            unityEvent.Invoke(arg0, arg1, arg2);
            ct.ThrowIfCancellationRequested();
        }
    }
}