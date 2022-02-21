using EmergenceEVMLocalServer.Controllers;
using EmergenceEVMLocalServer.Services;
using EmergenceEVMLocalServer.ViewModels;
using EmergenceSDK;
using Nethereum.Signer;
//using QRCoder;
using System;
using System.Threading.Tasks;

using GetBalanceResponse = EmergenceEVMLocalServer.ViewModels.GetBalanceResponse;
using IsConnectedResponse = EmergenceEVMLocalServer.ViewModels.IsConnectedResponse;
using ValidateAccessTokenResponse = EmergenceEVMLocalServer.ViewModels.ValidateAccessTokenResponse;

namespace NEthereumPoC.Controllers
{
    public class IntegrationLibraryController : EmergenceEVMController
    {

        private readonly WalletConnectSingleton _walletConnectSingleton;
        public IWeb3WalletConnectSingleton _web3WalletConnectSingleton;
        public IWeb3Service _web3Service;
        public ContractsService _contractsService;
        public AccountsService _accountsService;

        public IntegrationLibraryController(WalletConnectSingleton walletConnectSingleton, IWeb3WalletConnectSingleton web3WalletConnectSingleton, ContractsService contractsService, AccountsService accountsService, IWeb3Service webService)
        {
            _walletConnectSingleton = walletConnectSingleton;
            _web3WalletConnectSingleton = web3WalletConnectSingleton;
            _contractsService = contractsService;
            _accountsService = accountsService;
            _web3Service = webService;
        }

        // [HttpGet("getwalletconnecturi")]
        public string GetWalletConnectURI()
        {
            return _walletConnectSingleton.provider.URI;
        }

        // [HttpGet("qrcode")]
        /*public ActionResult GetQRCode()
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(GetWalletConnectURI(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(4);

                MemoryStream stream = new MemoryStream();
                qrCodeImage.Save(stream, ImageFormat.Png);

                return File(stream.ToArray(), "image/png");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }*/

        /// <summary>
        /// Attempts to initialize a session between the user's wallet and this server
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/handshake
        ///     {        
        ///       "nodeUrl": "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134",
        ///     }
        /// </remarks>
        // [HttpGet("handshake")]
        async public Task<string> WalletConnectHandShake(string nodeUrl)
        {
            if (_walletConnectSingleton.provider.Connected) return AlreadyConnected();

            if (_walletConnectSingleton.provider.Accounts != null)
                _walletConnectSingleton.reInitialize();

            try
            {
                await _walletConnectSingleton.provider.Connect();

                _web3WalletConnectSingleton.Initialize(_walletConnectSingleton.provider, nodeUrl);

                return SuccessResponse(new WalletConnectHandshakeResponse() { address = _walletConnectSingleton.provider.Accounts[0] });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        // [HttpGet("getbalance")]
        async public Task<string> GetBalance(string address, string nodeURL)
        {
            try
            {
                var web3 = _web3Service.TempWeb3Instance(nodeURL);
                var balance = await web3.Eth.GetBalance.SendRequestAsync(address);
                return SuccessResponse(new GetBalanceResponse() { balance = balance.Value.ToString() });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        // [HttpGet("isConnected")]
        public string IsConnected()
        {
            bool isConnected = _walletConnectSingleton.provider.Connected;
            bool connected = _walletConnectSingleton.provider.TransportConnected;
            return SuccessResponse(new IsConnectedResponse() { isConnected = isConnected, address = isConnected ? _walletConnectSingleton.provider.Accounts[0] : null });
        }

        // [HttpGet("reinitializewalletconnect")]
        public string ReinitializeWalletConnect()
        {
            _walletConnectSingleton.reInitialize();
            return SuccessResponse(new KillSessionResponse() { disconnected = true });
        }

        // [HttpGet("killSession")]
        async public Task<string> KillSession()
        {
            if (!_walletConnectSingleton.provider.Connected) return NotConnected();

            try
            {
                await _walletConnectSingleton.provider.Disconnect();
                await _walletConnectSingleton.provider.DisconnectSession();
                _walletConnectSingleton.provider.Dispose();
                _walletConnectSingleton.reInitialize();

                return SuccessResponse(new KillSessionResponse() { disconnected = true });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        /// <summary>
        /// Loads a contract in server's memory
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/loadContract
        ///     {        
        ///       "contractAddress": "0x7839d3bbf9ab26874c9a957Ad800EF68a2c6395f",
        ///       "ABI": "[{'inputs':[{'internalType':'string','name':'name','type':'string'},{'internalType':'string','name':'symbol','type':'string'}],'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'approved','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'owner','type':'address'},{'indexed':true,'internalType':'address','name':'operator','type':'address'},{'indexed':false,'internalType':'bool','name':'approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'TokenMinted','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'internalType':'address','name':'from','type':'address'},{'indexed':true,'internalType':'address','name':'to','type':'address'},{'indexed':true,'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'inputs':[{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'approve','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'}],'name':'balanceOf','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'owner','type':'address'},{'internalType':'address','name':'operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'player','type':'address'},{'internalType':'string','name':'tokenURI','type':'string'}],'name':'mint','outputs':[{'internalType':'uint256','name':'','type':'uint256'}],'stateMutability':'nonpayable','type':'function'},{'inputs':[],'name':'name','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'},{'internalType':'bytes','name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'address','name':'operator','type':'address'},{'internalType':'bool','name':'approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'stateMutability':'nonpayable','type':'function'},{'inputs':[{'internalType':'bytes4','name':'interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'internalType':'bool','name':'','type':'bool'}],'stateMutability':'view','type':'function'},{'inputs':[],'name':'symbol','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'tokenURI','outputs':[{'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function'},{'inputs':[{'internalType':'address','name':'from','type':'address'},{'internalType':'address','name':'to','type':'address'},{'internalType':'uint256','name':'tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'stateMutability':'nonpayable','type':'function'}]"
        ///     }
        /// </remarks>
        // [HttpPost("loadContract")]
        public string LoadContract(LoadContractRequest loadContractRequest)
        {
            try
            {
                _contractsService.LoadContract(loadContractRequest.contractAddress, loadContractRequest.ABI);
                return SuccessResponse();
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        /// <summary>
        /// Invoke a write method of a given contract address
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/writeMethod
        ///     {        
        ///       "contractAddress": "0x7839d3bbf9ab26874c9a957Ad800EF68a2c6395f",
        ///       "methodName": "mint",
        ///     }
        /// </remarks>
        // [HttpPost("writeMethod")]
        async public Task<string> writeMethod(string contractAddress, string methodName, string localAccountName = null, params string[] functionInput)
        {
            if (!_walletConnectSingleton.provider.Connected && _accountsService.accounts.Count == 0) return NotConnected();

            AccountsService.Account account = null;

            try
            {
                if (localAccountName != null)
                    account = _accountsService.GetAccount(localAccountName);

                var senderAddress = account is null ? _walletConnectSingleton.provider.Accounts[0] : _accountsService.GetAccount(localAccountName).wallet.Address;

                var contractMeta = _contractsService.GetContract(contractAddress);

                var contract = account is null ? _web3WalletConnectSingleton.provider.Eth.GetContract(contractMeta.ABI, contractMeta.address) : account.web3Provider.Eth.GetContract(contractMeta.ABI, contractMeta.address);

                var function = contract.GetFunction(methodName);

                var gas = await function.EstimateGasAsync(functionInput);

                string result;
                result = await function.SendTransactionAsync(senderAddress, gas, null, null, functionInput);

                return SuccessResponse(new WriteMethodResponse() { transactionHash = result });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        /// <summary>
        /// Invoke a read method of a given contract address
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/readMethod
        ///     {        
        ///       "contractAddress": "0x7839d3bbf9ab26874c9a957Ad800EF68a2c6395f",
        ///       "methodName": "tokenURI",
        ///     }
        /// </remarks>
        // [HttpPost("readMethod")]
        async public Task<string> readMethod(string contractAddress, string methodName, string localAccountName = null, params string[] functionInput)
        {
            if (!_walletConnectSingleton.provider.Connected && _accountsService.accounts.Count == 0) return NotConnected();

            try
            {
                var contractMeta = _contractsService.GetContract(contractAddress);

                Nethereum.Contracts.Contract contractInstance;

                if (localAccountName == null)
                    contractInstance = _web3WalletConnectSingleton.provider.Eth.GetContract(contractMeta.ABI, contractMeta.address);
                else
                    contractInstance = _accountsService.GetAccount(localAccountName).web3Provider.Eth.GetContract(contractMeta.ABI, contractMeta.address);

                var function = contractInstance.GetFunction(methodName);

                object result;

                result = await function.CallAsync<object>(functionInput);

                return SuccessResponse(new ReadMethodResponse() { response = result });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        /// <summary>
        /// Creates a new wallet and a keystore file. Stores the keystore file in the given location
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/createWallet
        ///     {        
        ///       "path": "c:\\wallets\\wallet1.json",
        ///       "password": "*****",
        ///     }
        /// </remarks>
        // [HttpPost("createWallet")]
        public string CreateWalletAndKeyStoreFile(string path, string password)
        {
            try
            {
                if (_accountsService.CreateWalletAndStoreKeyStoreFile(path, password))
                {
                    return SuccessResponse();
                }
                return ErrorResponse("Wallet file already exists", BaseResponse.StatusCode.FileAlreadyExists);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        /// <summary>
        /// Creates a key store for a given private key.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/createKeyStore
        ///     {        
        ///       "privateKey": "529889c7129cbeb7ef41edf8ae7f67337d4ea4b4a0783a3b00ef2319d246afdb",
        ///       "password": "test",
        ///       "publicKey": "0xae6d15962900Ba03aC171f976e9D116619e5252f",
        ///       "path": "c:\\wallets\\wallet2.json",
        ///     }
        /// </remarks>
        // [HttpPost("createKeyStore")]
        public string CreateKeyStoreFile(string path, string publicKey, string privateKey, string password)
        {
            try
            {
                if (_accountsService.CreateAndStoreKeyStoreFile(path, privateKey, password, publicKey))
                {
                    return SuccessResponse();
                }
                return ErrorResponse("Keystore file already exists", BaseResponse.StatusCode.FileAlreadyExists);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        /// <summary>
        /// Loads an account from a keystore file into the server. Also initializes a new Web3 instance for the given account. 
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/loadAccount
        ///     {        
        ///       "name": "DevAccount1",
        ///       "password": "test",
        ///       "path": "c:\\wallets\\wallet1.json",
        ///       "nodeURL": "https://polygon-mainnet.infura.io/v3/cb3531f01dcf4321bbde11cd0dd25134"
        ///     }
        /// </remarks>
        // [HttpPost("loadAccount")]
        public string LoadAccountFromKeyStoreFile(LoadAccountRequest request)
        {
            try
            {
                if (_accountsService.LoadWalletFromKeyStoreFile(request.name, request.password, request.path, request.nodeURL))
                {
                    return SuccessResponse();
                }
                return ErrorResponse("File not found", BaseResponse.StatusCode.FileNotFound);
            }
            catch (Exception e)
            {
                BaseResponse.StatusCode statusCode = BaseResponse.StatusCode.Error;
                if (e is Nethereum.KeyStore.Crypto.DecryptionException)
                {
                    statusCode = BaseResponse.StatusCode.IncorrectPassword;
                }

                return ErrorResponse(e.Message, statusCode);
            }
        }

        // [HttpGet("get-access-token")]
        async public Task<string> GetAccessToken()
        {
            if (!_walletConnectSingleton.provider.Connected) return NotConnected();

            try
            {
                string messageToSign = "{\"expires-on\":" + DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds() + "}";

                var senderAddress = _walletConnectSingleton.provider.Accounts[0];

                string result;

                result = await _walletConnectSingleton.provider.EthSign(senderAddress, messageToSign);

                return SuccessResponse(new GetAccessTokenResponse()
                {
                    accessToken = new AccessToken()
                    {
                        address = senderAddress,
                        message = messageToSign,
                        signedMessage = result
                    }
                });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        // [HttpGet("validate-access-token")]
        public string ValidateAccessToken(string accessToken)
        {
            try
            {
                var token = SerializationHelper.Deserialize<AccessToken>(accessToken);

                var signer = new EthereumMessageSigner();

                var result = signer.EncodeUTF8AndEcRecover(token.message, token.signedMessage);

                var date = SerializationHelper.Deserialize<MessageContent>(token.message);

                bool valid = result == token.address && date.expiresOn > DateTimeOffset.Now.ToUnixTimeSeconds();

                return SuccessResponse(new ValidateAccessTokenResponse() { valid = valid });
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        // [HttpGet("finish")]
        public void FinishApp()
        {
            //Program.StopHost();
        }
    }
}
