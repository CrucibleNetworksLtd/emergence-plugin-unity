using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Integrations.Futureverse.Internal.Services
{
    internal interface IWalletServiceInternal : IEmergenceService
    {
        /// <summary>
        /// Attempts to handshake with the Emergence server, retrieving the wallet address if successful.
        /// </summary>
        UniTask Handshake(HandshakeSuccess success, ErrorCallback errorCallback);
        /// <summary>
        /// Attempts to handshake with the Emergence server.
        /// </summary>
        UniTask<ServiceResponse<string>> HandshakeAsync();
        void RunWithSpoofedWalletAddress(string walletAddress, string checksummedWalletAddress, Action action);
        UniTask RunWithSpoofedWalletAddressAsync(string walletAddress, string checksummedWalletAddress, Func<UniTask> action);
    }
}