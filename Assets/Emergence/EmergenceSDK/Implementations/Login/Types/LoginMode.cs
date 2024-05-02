namespace EmergenceSDK.Implementations.Login.Types
{
    /// <summary>
    /// Represents the login path the <see cref="LoginManager"/> will follow.
    /// </summary>
    public enum LoginMode
    {
        /// <summary>
        /// Simple WalletConnect flow
        /// </summary>
        WalletConnect,
        /// <summary>
        /// WalletConnect flow with Futurepass
        /// </summary>
        Futurepass
    }
}