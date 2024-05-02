using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login.Events;
using EmergenceSDK.Implementations.Login.Exceptions;
using EmergenceSDK.Implementations.Login.Types;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace EmergenceSDK.Implementations.Login
{
    public sealed class LoginManager : MonoBehaviour
    {
        public bool IsLoggingIn { get; private set; }
        public EmergenceQrCode CurrentQrCode { get; internal set; }

        public LoginStartedEvent loginStartedEvent;
        public LoginCancelledEvent loginCancelledEvent;
        public LoginFailedEvent loginFailedEvent;
        public LoginSuccessfulEvent loginSuccessfulEvent;
        public LoginStepUpdatedEvent loginStepUpdatedEvent;
        public LoginEndedEvent loginEndedEvent;
        public QrCodeTickEvent qrCodeTickEvent;

        internal const float QrCodeTimeout = 60;

        private CancellationTokenSource _cts;
        private CancellationToken _ct;

        public async UniTask StartLogin(LoginMode loginMode)
        {
            if (IsLoggingIn) return;
            
            try
            {
                IsLoggingIn = true;
                var sessionServiceInternal = EmergenceServiceProvider.GetService<ISessionServiceInternal>();
                var walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
                var futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
                futureverseServiceInternal.UsingFutureverse = loginMode == LoginMode.Futurepass;
                var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
                _cts = new CancellationTokenSource();
                _ct = _cts.Token;

                InvokeEventAndCheckCancellationToken(loginStartedEvent, this, _ct);

                await HandleQrCodeRequest(sessionServiceInternal);
                await HandleHandshakeRequest(walletServiceInternal);
                await HandleAccessTokenRequest(sessionServiceInternal);
                await HandleFuturepassRequests(futureverseService);

                loginSuccessfulEvent.Invoke(this, ((IWalletService)walletServiceInternal).ChecksummedWalletAddress);
                sessionServiceInternal.RunConnectionEvents();
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
                loginEndedEvent.Invoke(this);
                IsLoggingIn = false;
            }
        }

        public void CancelLogin()
        {
            if (!IsLoggingIn) return;
            
            CurrentQrCode?.StopTicking();
            CurrentQrCode = null;
            _cts?.Cancel();
        }

        public UniTask WaitUntilAvailable(CancellationTokenSource cts = default)
        {
            // ReSharper disable once MethodSupportsCancellation
            return cts != null
                ? UniTask.WaitUntil(() => !IsLoggingIn, cancellationToken: cts.Token)
                : UniTask.WaitUntil(() => !IsLoggingIn);
        }

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

        #region Lifecycle

        private void OnDisable()
        {
            CancelLogin();
        }

        private void OnDestroy()
        {
            CancelLogin();
        }

        #endregion
        
        #region Helpers

        private void InvokeLoginFailedEvent(Exception e)
        {
            var loginExceptionContainer = new LoginExceptionContainer(e);
            loginFailedEvent.Invoke(this, loginExceptionContainer);
            loginExceptionContainer.ThrowIfUnhandled();
        }

        private async UniTask HandleFuturepassRequests(IFutureverseService futureverseService)
        {
            if (futureverseService?.UsingFutureverse == true)
            {
                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.FuturepassRequests, StepPhase.Start, _ct);

                ServiceResponse<LinkedFuturepassResponse> passResponse;
                try
                {
                    passResponse = await futureverseService.GetLinkedFuturepassAsync();
                    _ct.ThrowIfCancellationRequested();
                    if (!passResponse.Success)
                    {
                        throw new FuturepassRequestFailedException(passResponse);
                    }
                }
                catch (Exception e) when (e is not OperationCanceledException and not FuturepassRequestFailedException)
                {
                    throw new FuturepassRequestFailedException(e);
                }
                
                try
                {
                    var passInformationResponse = await futureverseService.GetFuturepassInformationAsync(passResponse.Result.ownedFuturepass);
                    _ct.ThrowIfCancellationRequested();
                    if (!passInformationResponse.Success)
                    {
                        throw new FuturepassInformationRequestFailedException(passInformationResponse);
                    }
                }
                catch (Exception e) when (e is not OperationCanceledException and not FuturepassInformationRequestFailedException)
                {
                    throw new FuturepassInformationRequestFailedException(e);
                }

                InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.FuturepassRequests, StepPhase.Success, _ct);
            }
        }

        private async UniTask HandleAccessTokenRequest(ISessionServiceInternal sessionServiceInternal)
        {
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.AccessTokenRequest, StepPhase.Start, _ct);
                
            try
            {
                var tokenResponse = await sessionServiceInternal.GetAccessTokenAsync();
                _ct.ThrowIfCancellationRequested();
                if (!tokenResponse.Success)
                {
                    throw new TokenRequestFailedException(tokenResponse);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException and not TokenRequestFailedException)
            {
                throw new TokenRequestFailedException(e);
            }

            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.AccessTokenRequest, StepPhase.Success, _ct);
        }

        private async UniTask HandleHandshakeRequest(IWalletServiceInternal walletServiceInternal)
        {
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.HandshakeRequest, StepPhase.Start, _ct);
                
            try
            {
                var handshakeResponse = await walletServiceInternal.HandshakeAsync(ct: _ct);
                _ct.ThrowIfCancellationRequested();
                if (!handshakeResponse.Success)
                {
                    throw new HandshakeRequestFailedException(handshakeResponse);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException and not HandshakeRequestFailedException)
            {
                throw new HandshakeRequestFailedException(e);
            }
            
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.HandshakeRequest, StepPhase.Success, _ct);
        }

        private async UniTask HandleQrCodeRequest(ISessionServiceInternal sessionServiceInternal)
        {
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.QrCodeRequest, StepPhase.Start, _ct);
                
            try
            {
                var qrCodeResponse = await sessionServiceInternal.GetQrCodeAsync(_ct);
                _ct.ThrowIfCancellationRequested();
                if (!qrCodeResponse.Success)
                {
                    throw new QrCodeRequestFailedException(qrCodeResponse);
                }
                CurrentQrCode = new EmergenceQrCode(this, qrCodeResponse.Result, EmergenceSingleton.Instance.CurrentDeviceId);
            }
            catch (Exception e) when (e is not OperationCanceledException and not QrCodeRequestFailedException)
            {
                throw new QrCodeRequestFailedException(e);
            }
            
            InvokeEventAndCheckCancellationToken(loginStepUpdatedEvent, this, LoginStep.QrCodeRequest, StepPhase.Success, _ct);

            // Single QR code tick when any UI elements should first update after retrieving QR.
            //qrCodeTickEvent.Invoke(this, CurrentQrCode);
        }

        #endregion
        
        #region EventInvokers

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

        #endregion
    }
}