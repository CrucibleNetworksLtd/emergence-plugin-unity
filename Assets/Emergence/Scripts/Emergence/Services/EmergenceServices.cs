using System;
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
            AccountService = gameObject.AddComponent<AccountService>();
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
        
        public void IsConnected(IsConnectedSuccess success, ErrorCallback errorCallback) => AccountService.IsConnected(success, errorCallback);
        
        public void ReinitializeWalletConnect(ReinitializeWalletConnectSuccess success, ErrorCallback errorCallback) => WalletService.ReinitializeWalletConnect(success, errorCallback);

        public void RequestToSign(string messageToSign, RequestToSignSuccess success, ErrorCallback errorCallback) => WalletService.RequestToSign(messageToSign, success, errorCallback);

        public void GetQRCode(QRCodeSuccess success, ErrorCallback errorCallback) => QRCodeService.GetQRCode(success, errorCallback);

        public void Handshake(HandshakeSuccess success, ErrorCallback errorCallback) => WalletService.Handshake(success, errorCallback);

        public void CreateWallet(string path, string password, CreateWalletSuccess success, ErrorCallback errorCallback) => WalletService.CreateWallet(path, password, success, errorCallback);

        public void CreateKeyStore(string privateKey, string password, string publicKey, string path,
            CreateKeyStoreSuccess success, ErrorCallback errorCallback) 
            => AccountService.CreateKeyStore(privateKey, password, publicKey, path, success, errorCallback);

        public void LoadAccount(string name, string password, string path, string nodeURL, string chainId,
            LoadAccountSuccess success, ErrorCallback errorCallback) 
            => AccountService.LoadAccount(name, password, path, nodeURL, chainId, success, errorCallback);

        public void GetBalance(BalanceSuccess success, ErrorCallback errorCallback)
        {
            if (skipWallet)
            {
                success?.Invoke("No wallet");
                return;
            }
            WalletService.GetBalance(success, errorCallback);
        }

        public void GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback) => AccountService.GetAccessToken(success, errorCallback);

        public void ValidateAccessToken(ValidateAccessTokenSuccess success, ErrorCallback errorCallback) => AccountService.ValidateAccessToken(success, errorCallback);

        public void ValidateSignedMessage(string message, string signedMessage, string address,
            ValidateSignedMessageSuccess success, ErrorCallback errorCallback)
            => AccountService.ValidateSignedMessage(message, signedMessage, address, success, errorCallback);

        public void Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            if (skipWallet)
            {
                success?.Invoke();
                return;
            }
            AccountService.Disconnect(success, errorCallback);
        }

        public void Finish(SuccessFinish success, ErrorCallback errorCallback) => AccountService.Finish(success, errorCallback);

        public void LoadContract(string contractAddress, string ABI, string network, LoadContractSuccess success,
            ErrorCallback errorCallback) 
            => ContractService.LoadContract(contractAddress, ABI, network, success, errorCallback);

        public void GetTransactionStatus<T>(string transactionHash, string nodeURL,
            GetTransactionStatusSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.GetTransactionStatus(transactionHash, nodeURL, success, errorCallback);

        public void GetBlockNumber<T, U>(string transactionHash, string nodeURL, U body,
            GetBlockNumberSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.GetBlockNumber(transactionHash, nodeURL, body, success, errorCallback);
        
        public void ReadMethod<T, U>(string contractAddress, string methodName, string network, string nodeUrl, U body, ReadMethodSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.ReadMethod(contractAddress, methodName, network, nodeUrl, body, success, errorCallback);

        public void WriteMethod<T, U>(string contractAddress, string methodName, string localAccountName,
            string gasPrice, string network, string nodeUrl, string value, U body, WriteMethodSuccess<T> success, ErrorCallback errorCallback) 
            => ContractService.WriteMethod(contractAddress, methodName, localAccountName, gasPrice, network, nodeUrl, value, body, success, errorCallback);

    }
}