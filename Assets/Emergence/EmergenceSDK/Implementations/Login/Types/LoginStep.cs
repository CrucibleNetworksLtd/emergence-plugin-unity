namespace EmergenceSDK.Implementations.Login.Types
{
    /// <summary>
    /// Represents each step of the login flow
    /// </summary>
    public enum LoginStep
    {
        /// <summary>
        /// Requesting the QR code.<para/>
        /// When this step succeeds we can obtain the QR code texture from the <see cref="LoginManager"/> via <see cref="LoginManager.CurrentQrCode"/>, and <see cref="LoginManager.qrCodeTickEvent"/> will start ticking.
        /// </summary>
        QrCodeRequest,
        /// <summary>
        /// Performing the handshake, waiting for user to scan the QR code
        /// </summary>
        HandshakeRequest,
        /// <summary>
        /// Requesting the access token from Emergence
        /// </summary>
        AccessTokenRequest,
        /// <summary>
        /// Requesting the Futurepass information
        /// </summary>
        FuturepassRequests
    }
}