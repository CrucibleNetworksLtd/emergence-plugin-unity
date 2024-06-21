using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Internal.Services
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