using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Services;

namespace EmergenceSDK.Runtime.Internal.Services
{   
    /// <summary>
    /// An Emergence service used for Custodial signing of Web3 interactions.
    /// </summary>
    public interface ICustodialSigningService : IEmergenceService
    {
        /// <summary>
        /// UniTask for handling requests to proccess messages and send them to an external signing service.
        /// </summary>
        /// <param name="custodialEOA">The Custodial EOA or wallet to sign the message.</param>
        /// <param name="messageToSign">The message to be signed.</param>
        /// <param name="timestamp">The timestamp of the message being signed.</param>
        /// <returns></returns>
        UniTask<string> RequestToSignAsync(string custodialEOA, string messageToSign, string timestamp);
    }
}