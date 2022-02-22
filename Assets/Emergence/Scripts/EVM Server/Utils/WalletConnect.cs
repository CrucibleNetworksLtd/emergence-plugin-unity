using Nethereum.JsonRpc.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.NEthereum;
using WCS = WalletConnectSharp.Unity.WalletConnect;
namespace EmergenceSDK
{
    /// <summary>
    /// Translation layer for WalletConnectSharp.Unity.WalletConnect to WalletConnectSharp.Desktop
    /// which the EVM server uses
    /// </summary>
    public class WalletConnect
    {
        public bool Connected
        {
            get
            {
                return true;
            }
        }

        public string URI
        {
            get
            {
                return "";
            }
        }

        public string[] Accounts
        {
            get;
            set;
        }

        public bool TransportConnected
        {
            get
            {
                return true;
            }
        }

        public async Task<string> EthSign(string address, string message, Encoding messageEncoding = null)
        {
            return await WCS.Instance.Session.EthSign(address, message);
        }


        public WalletConnect(ClientMeta clientMeta)
        {
            if (WCS.Instance != null)
            {
                WCS.Instance.CloseSession(true);
            }
        }

        public async Task Connect()
        {
            await WCS.Instance.Connect();
        }

        public async Task Disconnect()
        {
            await Task.Run(() => WCS.Instance.CloseSession(false));
        }

        public async Task DisconnectSession()
        {
            await Task.Run(() => WCS.Instance.CloseSession(false));
        }

        public void Dispose()
        {

        }

        public IClient CreateProvider(Uri uri)
        {
            var wcProtocol = WCS.Instance.Session;
            return wcProtocol.CreateProvider(uri);
        }

        /*
         *             //var web3 = new Web3(wcProtocol.CreateProvider(new Uri("https://mainnet.infura.io/v3/<infruaId>"));
            //web3Provider = new Web3(_walletConnect.CreateProvider(new Uri(nodeURI)));

            var wcProtocol = WalletConnect.Instance.Session;
            web3Provider = new Web3(wcProtocol.CreateProvider(new Uri(nodeURI)));*/
    }
}
