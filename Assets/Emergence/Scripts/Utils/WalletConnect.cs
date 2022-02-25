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
                bool connected = false;
                if (WCS.Instance.Session != null)
                {
                    connected = WCS.Instance.Session.Connected;
                }
                return connected;
            }
        }

        public string URI
        {
            get
            {
                string uri = string.Empty;
                if (WCS.Instance.Session != null)
                {
                    uri = WCS.Instance.Session.URI;
                }
                return uri;
            }
        }

        public string[] Accounts
        {
            get
            {
                string[] accounts = null;
                if (WCS.Instance.Session != null)
                {
                    accounts = WCS.Instance.Session.Accounts;
                }
                return accounts;
            }
        }

        public bool TransportConnected
        {
            get
            {
                bool connected = false;
                if (WCS.Instance.Session != null)
                {
                    connected = WCS.Instance.Session.TransportConnected;
                }
                return connected;
            }
        }

        public async Task<string> EthSign(string address, string message, Encoding messageEncoding = null)
        {
            return await WCS.Instance.Session.EthSign(address, message);
        }

        public void SetNodeURL(string nodeURL)
        {
            WCS.Instance.customBridgeUrl = nodeURL;
        }

        private ClientMeta clientMeta;
        public WalletConnect(ClientMeta clientMeta)
        {
            this.clientMeta = clientMeta;
            SetValues();
            if (Services.ShouldReinitialize)
            {
                ReconnectLocal();
            }
            else
            {
                DisconnectLocal();
            }
        }

        private void SetValues()
        {
            WCS.Instance.AppData.Description = clientMeta.Description;
            WCS.Instance.AppData.URL = clientMeta.URL;
            WCS.Instance.AppData.Icons = clientMeta.Icons;
            WCS.Instance.AppData.Name = clientMeta.Name;

            DisconnectLocal();
        }

        private async void DisconnectLocal()
        {
            await Task.Run(() => WCS.Instance.CloseSession(true));
        }

        private async void ReconnectLocal()
        {
            SetValues();
            await Connect();
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
            // No need
        }

        public IClient CreateProvider(Uri uri)
        {
            var wcProtocol = WCS.Instance.Session;
            return wcProtocol.CreateProvider(uri);
        }
    }
}
