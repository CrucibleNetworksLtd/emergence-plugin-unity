using System;

namespace EmergenceSDK
{
    public class EnvValues
    {
        public string APIBase;
        public string defaultNodeURL;
        public string databaseAPIPrivate;
        public string IPFSNode;
        public string CustomEmergenceServerLocation;
        public string CustomEmergenceServerURL;

        internal static EnvValues MapFrom(EmergenceConfiguration configuration)
        {
            EnvValues envValues = new EnvValues
            {
                CustomEmergenceServerLocation = configuration.CustomEmergenceServerLocation,
                APIBase = configuration.APIBase,
                CustomEmergenceServerURL = configuration.CustomEmergenceServerURL,
                databaseAPIPrivate = configuration.databaseAPIPrivate,
                defaultNodeURL = configuration.databaseAPIPrivate,
                IPFSNode = configuration.databaseAPIPrivate

            };
            return envValues;
        }
    }
}