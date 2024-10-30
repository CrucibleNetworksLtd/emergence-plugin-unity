using System;

namespace EmergenceSDK.Runtime.Types
{
    /// <summary>
    /// Settings for the login path that the <see cref="LoginManager"/> will follow.
    /// </summary>
    [Flags]
    public enum LoginSettings
    {
        /// <summary>
        /// Default login settings
        /// </summary>
        Default = 0,
        /// <summary>
        /// Disable retrieving the access token for Emergence services requiring authentication, e.g.: personas
        /// </summary>
        DisableEmergenceAccessToken = 1,
        /// <summary>
        /// Enable retrieving Futurepass for the wallet and storing it
        /// </summary>
        EnableFuturepass = 2,
        
        EnableCustodialLogin = 4
    }
}