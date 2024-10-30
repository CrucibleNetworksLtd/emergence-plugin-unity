using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using System.Threading;

namespace EmergenceSDK.Runtime.Services
{
    public interface ICustodialLoginService : IEmergenceService
    {
        /// <summary>
        /// Starts the custodial login process, generating state, code challenge, 
        /// and creating the authorization URL. Initiates the local web server to handle the callback.
        /// </summary>
        UniTask<ServiceResponse<string>> StartCustodialLoginAsync(Action<string,CancellationToken> onSuccessfulLogin,CancellationToken ct);
    }
}