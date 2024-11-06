using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;

namespace EmergenceSDK.Runtime.Internal.Services
{
    public interface ICustodialSigningService : IEmergenceService
    {
        /// <summary>
        /// Initiates the signing process for a message using the custodial service.
        /// </summary>
        /// <param name="custodialEOA">The Ethereum address for the custodial account.</param>
        /// <param name="message">The message to be signed.</param>
        /// <param name="ct">Cancellation token for managing async flow cancellation.</param>
        /// <returns>A UniTask containing the signed message.</returns>
        UniTask<ServiceResponse<string>> SignMessageAsync(string custodialEOA, string message, CancellationToken ct);
    }
}