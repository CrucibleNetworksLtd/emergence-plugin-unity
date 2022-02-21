using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore.Model;
using Nethereum.Web3;
using System.Collections.Generic;

namespace EmergenceEVMLocalServer.Services
{
    public class AccountsService
    {
        public void InitializeWeb3(Account account, string nodeURL)
        {
            account.web3Provider = new Web3(account.wallet, nodeURL);
        }

        public class Account
        {
            public string name { get; set; }
            public Nethereum.Web3.Accounts.Account wallet { get; set; }
            public Web3 web3Provider { get; set; }
        }

        public List<Account> accounts = new List<Account>();

        public bool LoadWalletFromKeyStoreFile(string name, string password, string path, string nodeURL)
        {
            if (!System.IO.File.Exists(path))
            {
                return false;
            }

            var keyStoreFile = System.IO.File.ReadAllText(path);

            var wallet = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(keyStoreFile, password);

            if (accounts.Exists(x => x.name == name))
                accounts.Remove(accounts.Find(x => x.name == name));

            accounts.Add(new Account() { name = name, wallet = wallet });

            InitializeWeb3(GetAccount(name), nodeURL);

            return true;
        }

        public bool CreateWalletAndStoreKeyStoreFile(string path, string password)
        {
            var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();

            // parameters to encrypt our file using scrypt
            var scryptParams = new ScryptParams { Dklen = 32, N = 262144, R = 1, P = 8 };

            // generates an Ethereum compliant privateKey:
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

            // encrypt and serialize
            var keyStore = keyStoreService.EncryptAndGenerateKeyStore(password, ecKey.GetPrivateKeyAsBytes(), ecKey.GetPublicAddress(), scryptParams);
            var json = keyStoreService.SerializeKeyStoreToJson(keyStore);

            return Utils.FileUtils.SaveStringIfFileDoesNotExist(path, json);
        }

        public bool CreateAndStoreKeyStoreFile(string path, string privateKey, string password, string publicKey)
        {
            var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();

            // encrypt and serialize
            var keyStore = keyStoreService.EncryptAndGenerateKeyStore(password, privateKey.HexToByteArray(), publicKey);
            var json = keyStoreService.SerializeKeyStoreToJson(keyStore);

            return Utils.FileUtils.SaveStringIfFileDoesNotExist(path, json);
        }

        public Account GetAccount(string name)
        {
            return accounts.Find(x => x.name == name);
        }

    }
}