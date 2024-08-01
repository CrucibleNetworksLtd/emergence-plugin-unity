using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;

namespace EmergenceSDK.Runtime.Internal.Services
{
    internal interface IWalletServiceInternal : IEmergenceService
    {
        /// <summary>
        /// Attempts to handshake with the Emergence server, retrieving the wallet address if successful.
        /// </summary>
        UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback, float timeout = WebRequestService.DefaultTimeoutMilliseconds, CancellationCallback cancellationCallback = default, CancellationToken ct = default);
        /// <summary>
        /// Attempts to handshake with the Emergence server.
        /// </summary>
        UniTask<ServiceResponse<string>> HandshakeAsync(float timeout = WebRequestService.DefaultTimeoutMilliseconds, CancellationToken ct = default);
    }
}