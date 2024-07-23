using System;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using NUnit.Framework;

namespace EmergenceSDK.Tests.UnitTests.Emergence.Services.WalletService
{
    [TestFixture]
    public class WalletServiceTests
    {
        private IWalletServiceDevelopmentOnly walletServiceDevelopmentOnly;
        private IWalletService walletService;
        private IDisposable verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            verboseOutput = EmergenceLogger.VerboseOutput(true);
            EmergenceServiceProvider.Load(ServiceProfile.Default);
            walletService = EmergenceServiceProvider.GetService<IWalletService>();
            walletServiceDevelopmentOnly = EmergenceServiceProvider.GetService<IWalletServiceDevelopmentOnly>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            verboseOutput?.Dispose();
            EmergenceServiceProvider.Unload();
        }

        [Test]
        public void SpoofedWallet_SpoofsSuccessfully()
        {
            using var spoofedWallet = walletServiceDevelopmentOnly.SpoofedWallet("abcdefg", "aBcDeFg");
            Assert.AreEqual("abcdefg", walletService.WalletAddress, "Spoofed wallet did not set");
            Assert.AreEqual("aBcDeFg", walletService.ChecksummedWalletAddress, "Spoofed checksummed wallet did not set");
        }

        [Test]
        public void SpoofedWallet_ResetsSuccessfully()
        {
            var prevWalletAddress = walletService.WalletAddress;
            var prevChecksummedWalletAddress = walletService.ChecksummedWalletAddress;
            using (walletServiceDevelopmentOnly.SpoofedWallet("abcdefg", "aBcDeFg"))
            {
                Assert.AreEqual("abcdefg", walletService.WalletAddress, "Spoofed wallet did not set");
                Assert.AreEqual("aBcDeFg", walletService.ChecksummedWalletAddress, "Spoofed checksummed wallet did not set");
            }
            Assert.AreEqual(prevWalletAddress, walletService.WalletAddress, "Spoofed wallet did not reset");
            Assert.AreEqual(prevChecksummedWalletAddress, walletService.ChecksummedWalletAddress, "Spoofed checksummed wallet did not reset");
        }
    }
}