using System;
using EmergenceSDK.Services;
using UnityEngine.Events;

namespace EmergenceSDK.Implementations.Login.Events
{
    /// <summary>
    /// <list type="bullet">
    /// <listheader><term>Passed Parameters</term></listheader>
    /// <item><description><see cref="LoginManager"/> - The <see cref="LoginManager"/> that fired this event</description></item>
    /// <item><description><see cref="string"/> - The <see cref="IWalletService.ChecksummedWalletAddress"/> of the successfully connected wallet, obtained from <see cref="IWalletService"/></description></item>
    /// </list>
    /// <seealso cref="IWalletService"/>
    /// </summary>
    [Serializable] public sealed class LoginSuccessfulEvent : UnityEvent<LoginManager, string> {}
}