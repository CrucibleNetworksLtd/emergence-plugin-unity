using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Implementations.Login;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
{
    internal interface IWalletServiceInternal : IEmergenceService
    {
        IDisposable SpoofedWallet(string wallet, string checksummedWallet);
        /// <summary>
        /// Attempts to handshake with the Emergence server, retrieving the wallet address if successful.
        /// </summary>
        UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback, float timeout = LoginManager.QrCodeTimeout * 1000, CancellationCallback cancellationCallback = default, CancellationToken ct = default);
        /// <summary>
        /// Attempts to handshake with the Emergence server.
        /// </summary>
        UniTask<ServiceResponse<string>> HandshakeAsync(float timeout = LoginManager.QrCodeTimeout * 1000, CancellationToken ct = default);

        /// <summary>
        /// Performs an action while the wallet service thinks another specified wallet address is currently cached in
        /// <remarks>THIS IS A TEMPORARY DEVELOPER FEATURE, MEANT ONLY FOR TESTING.<para/>This will not work at all with any write requests, and has unexpected results for read requests as well.</remarks>
        /// </summary>
        /// <param name="walletAddress">Spoofed wallet address</param>
        /// <param name="checksummedWalletAddress">Spoofed checksummed wallet address</param>
        /// <param name="action">Closure to run</param>
        [Obsolete]
        public void RunWithSpoofedWalletAddress(string walletAddress, string checksummedWalletAddress, Action action);

        /// <summary>
        /// Performs an action asynchronously while the wallet service thinks another specified wallet address is currently cached in
        /// <remarks>THIS IS A TEMPORARY DEVELOPER FEATURE, MEANT ONLY FOR TESTING.<para/>This will not work at all with any write requests, and has unexpected results for read requests as well.</remarks>
        /// </summary>
        /// <param name="walletAddress">Spoofed wallet address</param>
        /// <param name="checksummedWalletAddress">Spoofed checksummed wallet address</param>
        /// <param name="action">Async closure to run</param>
        /// <returns></returns>
        [Obsolete]
        public UniTask RunWithSpoofedWalletAddressAsync(string walletAddress, string checksummedWalletAddress, Func<UniTask> action);
    }
}