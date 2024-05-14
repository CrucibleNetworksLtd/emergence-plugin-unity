using System;
using EmergenceSDK.Services;

namespace EmergenceSDK.Types.Exceptions
{
    /// <summary>
    /// Thrown when the <see cref="LoginSettings.DisableEmergenceAccessToken"/> flag should not be set in <see cref="ISessionService.CurrentLoginSettings"/>, yet it is found.
    /// </summary>
    public class EmergenceAccessTokenDisabledException : Exception { }
}