namespace EmergenceSDK.Runtime.Internal.Utils
{
    public static class StaticConfig
    {
        /// <summary>
        /// URL pointing to the Emergence API.
        /// <remarks>Changing this will break Emergence</remarks>
        /// </summary>
        public const string APIBase = "https://evm6.openmeta.xyz/api/";

        public const string HasLoggedInOnceKey = "HasLoggedInOnce";
    }
}