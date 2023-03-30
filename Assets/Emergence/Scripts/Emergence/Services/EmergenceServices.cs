using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Services;
using EmergenceSDK.Internal.UI;
using EmergenceSDK.Internal.UI.Screens;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Services
{
    /// <summary>
    /// The services singleton provides you with all the methods you need to get going with Emergence.
    /// </summary>
    /// <remarks>See our prefabs for examples of how to use it!</remarks>
    public class EmergenceServices : MonoBehaviour
    {
        public string CurrentAccessToken => AccountService.CurrentAccessToken;
        
        public bool DisconnectInProgress => AccountService.DisconnectInProgress;

        public static EmergenceServices Instance;

        public IPersonaService PersonaService { get; private set; }

        public IAvatarService AvatarService { get; private set; }
        
        public IInventoryService InventoryService { get; private set; }
        
        public IDynamicMetadataService DynamicMetadataService { get; private set; }
        
        public IAccountService AccountService { get; private set; }
        
        public IWalletService WalletService { get; private set; }
        
        public IQRCodeService QRCodeService { get; private set; }
        
        public IContractService ContractService { get; private set; }

        private bool skipWallet = false;
        
        public Expiration expiration;
        public class Expiration
        {
            [JsonProperty("expires-on")]
            public long expiresOn;
        }
        
        private void Awake()
        {
            Instance = this;
            PersonaService = gameObject.AddComponent<PersonaService>();
            AvatarService = new AvatarService();
            InventoryService = new InventoryService();
            DynamicMetadataService = new DynamicMetadataService();
            AccountService = new AccountService();
            WalletService = gameObject.AddComponent<WalletService>();
            QRCodeService = gameObject.AddComponent<QRCodeService>();
            ContractService = gameObject.AddComponent<ContractService>();
        }

        private bool refreshingToken = false;
        private void Update()
        {
            if (ScreenManager.Instance == null)
            {
                return;
            }

            bool uiIsVisible = ScreenManager.Instance.gameObject.activeSelf;

            if (!skipWallet && uiIsVisible && !refreshingToken && AccountService.HasAccessToken)
            {
                long now = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (expiration.expiresOn - now < 0)
                {
                    refreshingToken = true;
                    ModalPromptOK.Instance.Show("Token expired. Check your wallet for renewal", () =>
                    {
                        GetAccessToken((token) =>
                        {
                            refreshingToken = false;
                        },
                        (error, code) =>
                        {
                            Debug.LogError("[" + code + "] " + error);
                            refreshingToken = false;
                        });
                    });
                }
            }
        }
        
        public void SkipWallet(bool skip, string accessTokenJson)
        {
            skipWallet = skip;

            BaseResponse<AccessTokenResponse> response = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(accessTokenJson);
            AccountService.CurrentAccessToken = SerializationHelper.Serialize(response.message.AccessToken, false);
            EmergenceUtils.ProcessExpiration(response.message.AccessToken.message);
        }
        
        /// <inheritdoc cref="IPersonaService.GetPersonas"/>
        public void GetPersonas(SuccessPersonas success, ErrorCallback errorCallback) => PersonaService.GetPersonas(success, errorCallback);
        
        /// <inheritdoc cref="IPersonaService.GetCurrentPersona(SuccessGetCurrentPersona, ErrorCallback)"/>
        public void GetCurrentPersona(SuccessGetCurrentPersona success, ErrorCallback errorCallback) => PersonaService.GetCurrentPersona(success, errorCallback);
        
        /// <inheritdoc cref="IPersonaService.CreatePersona"/>
        public void CreatePersona(Persona persona, SuccessCreatePersona success, ErrorCallback errorCallback) => PersonaService.CreatePersona(persona, success, errorCallback);
        
        /// <inheritdoc cref="IPersonaService.EditPersona"/>
        public void EditPersona(Persona persona, SuccessEditPersona success, ErrorCallback errorCallback) => PersonaService.EditPersona(persona, success, errorCallback);
        
        /// <inheritdoc cref="IPersonaService.DeletePersona"/>
        public void DeletePersona(Persona persona, SuccessDeletePersona success, ErrorCallback errorCallback) => PersonaService.DeletePersona(persona, success, errorCallback);
        
        /// <inheritdoc cref="IPersonaService.SetCurrentPersona"/>
        public void SetCurrentPersona(Persona persona, SuccessSetCurrentPersona success, ErrorCallback errorCallback) => PersonaService.SetCurrentPersona(persona, success, errorCallback);
        
        /// <inheritdoc cref="IAvatarService.AvatarByOwner"/>
        public async void AvatarByOwner(string address, SuccessAvatars success, ErrorCallback errorCallback) => await AvatarService.AvatarByOwner(address, success, errorCallback);

        /// <inheritdoc cref="IAvatarService.AvatarById"/>
        public async void AvatarById(string id, SuccessAvatar success, ErrorCallback errorCallback) => await AvatarService.AvatarById(id, success, errorCallback);

        /// <inheritdoc cref="IInventoryService.InventoryByOwner"/>
        public async void InventoryByOwner(string address, SuccessInventoryByOwner success, ErrorCallback errorCallback) => await InventoryService.InventoryByOwner(address, success, errorCallback);

        /// <inheritdoc cref="IDynamicMetadataService.WriteDynamicMetadata"/>
        public async void WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback) => await DynamicMetadataService.WriteDynamicMetadata(network, contract, tokenId, metadata, success, errorCallback);
        
        /// <inheritdoc cref="IAccountService.IsConnected"/>
        public async void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback) => await AccountService.IsConnected(success, errorCallback);
        
        /// <inheritdoc cref="IWalletService.ReinitializeWalletConnect"/>
        public void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback) => WalletService.ReinitializeWalletConnect(success, errorCallback);

        /// <inheritdoc cref="IWalletService.RequestToSign"/>
        public void RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback) => WalletService.RequestToSign(messageToSign, success, errorCallback);

        /// <inheritdoc cref="IQRCodeService.GetQRCode"/>
        public void GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback) => QRCodeService.GetQRCode(success, errorCallback);

        /// <inheritdoc cref="IWalletService.Handshake"/>
        public void Handshake(HandshakeSuccess success, ErrorCallback errorCallback) => WalletService.Handshake(success, errorCallback);

        /// <inheritdoc cref="IWalletService.CreateWallet"/>
        public void CreateWallet(string path, string password, CreateWalletSuccess success, ErrorCallback errorCallback) => WalletService.CreateWallet(path, password, success, errorCallback);

        /// <inheritdoc cref="IAccountService.CreateKeyStore"/>
        public async void CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback) 
            => await AccountService.CreateKeyStore(privateKey, password, publicKey, path, success, errorCallback);

        /// <inheritdoc cref="IAccountService.LoadAccount"/>
        public async void LoadAccount(Account account, LoadAccountSuccess success, ErrorCallback errorCallback) 
            => await AccountService.LoadAccount(account, success, errorCallback);

        /// <inheritdoc cref="IWalletService.GetBalance"/>
        public void GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            if (skipWallet)
            {
                success?.Invoke("No wallet");
                return;
            }
            WalletService.GetBalance(success, errorCallback);
        }

        /// <inheritdoc cref="IAccountService.GetAccessToken"/>
        public async void GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback) 
            => await AccountService.GetAccessToken(success, errorCallback);

        /// <inheritdoc cref="IAccountService.ValidateAccessToken"/>
        public async void ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback) 
            => await AccountService.ValidateAccessToken(success, errorCallback);

        /// <inheritdoc cref="IAccountService.ValidateSignedMessage"/>
        public async void ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback)
            => await AccountService.ValidateSignedMessage(message, signedMessage, address, success, errorCallback);

        /// <inheritdoc cref="IAccountService.Disconnect"/>
        public async void Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            if (skipWallet)
            {
                success?.Invoke();
                return;
            }
            await AccountService.Disconnect(success, errorCallback);
        }

        /// <inheritdoc cref="IAccountService.Finish"/>
        public async void Finish(SuccessFinish success, ErrorCallback errorCallback) 
            => await AccountService.Finish(success, errorCallback);

        /// <inheritdoc cref="IContractService"/>
        public void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success,
            ErrorCallback errorCallback) 
            => ContractService.LoadContract(contractAddress, ABI, network, success, errorCallback);

        /// <inheritdoc cref="IContractService.GetTransactionStatus{T}"/>
        public void GetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.GetTransactionStatus(transactionHash, nodeURL, success, errorCallback);

        /// <inheritdoc cref="IContractService.GetBlockNumber{T,U}"/>
        public void GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.GetBlockNumber(transactionHash, nodeURL, body, success, errorCallback);
        
        /// <inheritdoc cref="IContractService.ReadMethod{T,U}"/>
        public void ReadMethod<T, U>(ContractInfo contractInfo, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.ReadMethod(contractInfo, body, success, errorCallback);

        /// <inheritdoc cref="IContractService.WriteMethod{T,U}"/>
        public void WriteMethod<T, U>(ContractInfo contractInfo, string localAccountName, string gasPrice, string value,
            U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.WriteMethod(contractInfo, localAccountName, gasPrice, value, body, success, errorCallback);

    }
}