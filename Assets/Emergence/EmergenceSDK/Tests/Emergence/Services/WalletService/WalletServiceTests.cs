using System;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using NUnit.Framework;

namespace EmergenceSDK.Tests.Emergence.Services.WalletService
{
    [TestFixture]
    public class WalletServiceTests
    {
        private IWalletServiceInternal _walletServiceInternal;
        private IWalletService _walletService;
        private IDisposable _verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            _verboseOutput = EmergenceLogger.VerboseOutput(true);
            EmergenceServiceProvider.Load();
            _walletService = EmergenceServiceProvider.GetService<IWalletService>();
            _walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _verboseOutput?.Dispose();
            EmergenceServiceProvider.Unload();
        }

        [Test]
        public void SpoofedWallet_SpoofsSuccessfully()
        {
            using var spoofedWallet = _walletServiceInternal.SpoofedWallet("abcdefg", "aBcDeFg");
            Assert.AreEqual("abcdefg", _walletService.WalletAddress, "Spoofed wallet did not set");
            Assert.AreEqual("aBcDeFg", _walletService.ChecksummedWalletAddress, "Spoofed checksummed wallet did not set");
        }

        [Test]
        public void SpoofedWallet_ResetsSuccessfully()
        {
            var prevWalletAddress = _walletService.WalletAddress;
            var prevChecksummedWalletAddress = _walletService.ChecksummedWalletAddress;
            using (_walletServiceInternal.SpoofedWallet("abcdefg", "aBcDeFg"))
            {
                Assert.AreEqual("abcdefg", _walletService.WalletAddress, "Spoofed wallet did not set");
                Assert.AreEqual("aBcDeFg", _walletService.ChecksummedWalletAddress, "Spoofed checksummed wallet did not set");
            }
            Assert.AreEqual(prevWalletAddress, _walletService.WalletAddress, "Spoofed wallet did not reset");
            Assert.AreEqual(prevChecksummedWalletAddress, _walletService.ChecksummedWalletAddress, "Spoofed checksummed wallet did not reset");
        }
    }
}