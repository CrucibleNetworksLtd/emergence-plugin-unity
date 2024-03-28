namespace EmergenceSDK.Internal.Utils
{
    public static class StaticConfig
    {
        /// <summary>
        /// URL pointing to the Emergence API.
        /// <remarks>Changing this will break Emergence</remarks>
        /// </summary>
        public const string APIBase = "https://evm3.openmeta.xyz/api/";

        public const string HasLoggedInOnceKey = "HasLoggedInOnce";
    }
}