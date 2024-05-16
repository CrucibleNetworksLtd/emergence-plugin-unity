using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Delegates;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// Interface containing public members for the wallet service that are meant for development ONLY
    /// </summary>
    public interface IWalletServiceDevelopmentOnly : IEmergenceService
    {
        /// <summary>
        /// <see cref="IDisposable"/> object that will keep a spoofed wallet active until disposed.
        /// <remarks>THIS IS A DEVELOPER FEATURE, MEANT ONLY FOR TESTING.<para/>Use with "using" keyword is strongly recommended for easiest management<para/>This will not work at all with any write requests, and has unexpected results for read requests as well.</remarks>
        /// </summary>
        /// <param name="wallet">Wallet address to spoof</param>
        /// <param name="checksummedWallet">Checksummed wallet address to spoof</param>
        /// <returns></returns>
        IDisposable SpoofedWallet(string wallet, string checksummedWallet);

        /// <summary>
        /// Performs an action while the wallet service thinks another specified wallet address is currently cached in
        /// <remarks>OBSOLETE. SEE <see cref="SpoofedWallet"/>.<para/>THIS IS A DEVELOPER FEATURE, MEANT ONLY FOR TESTING.<para/>This will not work at all with any write requests, and has unexpected results for read requests as well.</remarks>
        /// </summary>
        /// <param name="walletAddress">Spoofed wallet address</param>
        /// <param name="checksummedWalletAddress">Spoofed checksummed wallet address</param>
        /// <param name="action">Closure to run</param>
        [Obsolete]
        public void RunWithSpoofedWalletAddress(string walletAddress, string checksummedWalletAddress, Action action);

        /// <summary>
        /// Performs an action asynchronously while the wallet service thinks another specified wallet address is currently cached in
        /// <remarks>OBSOLETE. SEE <see cref="SpoofedWallet"/>.<para/>THIS IS A DEVELOPER FEATURE, MEANT ONLY FOR TESTING.<para/>This will not work at all with any write requests, and has unexpected results for read requests as well.</remarks>
        /// </summary>
        /// <param name="walletAddress">Spoofed wallet address</param>
        /// <param name="checksummedWalletAddress">Spoofed checksummed wallet address</param>
        /// <param name="action">Async closure to run</param>
        /// <returns></returns>
        [Obsolete]
        public UniTask RunWithSpoofedWalletAddressAsync(string walletAddress, string checksummedWalletAddress, Func<UniTask> action);
    }
}