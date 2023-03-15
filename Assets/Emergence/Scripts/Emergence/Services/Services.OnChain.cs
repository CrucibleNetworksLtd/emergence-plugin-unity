using System;
using System.Collections;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using EmergenceSDK.Types.Responses;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace EmergenceSDK.Services
{
    public partial class EmergenceServices
    {
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