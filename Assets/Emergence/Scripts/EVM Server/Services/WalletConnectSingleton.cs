using EmergenceSDK;
using System;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;

namespace EmergenceEVMLocalServer.Services
{
    public class WalletConnectSingleton
    {
        private WalletConnect _walletConnectProvider;

        ClientMeta clientMeta;

        public class ClientMetaBody
        {
            public string Name { get; set; }
            public string URL { get; set; }
            public string Icons { get; set; }
            public string Description { get; set; }
        }

        public WalletConnectSingleton(string clientMetadataBody)
        {
            ClientMetaBody clientMetaBody;

            try
            {
                clientMetaBody = SerializationHelper.Deserialize<ClientMetaBody>(clientMetadataBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("Unable to deserialize client metadata body");
            }

            clientMeta = new ClientMeta()
            {
                Name = clientMetaBody.Name,
                URL = clientMetaBody.URL,
                Icons = new[] { clientMetaBody.Icons },
                Description = clientMetaBody.Description
            };

            _walletConnectProvider = new WalletConnect(clientMeta);
        }

        public void reInitialize()
        {
            _walletConnectProvider = null;
            _walletConnectProvider = new WalletConnect(clientMeta);
        }

        public WalletConnect provider { get => _walletConnectProvider; }

    }
}
