using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EmergenceSDK.Tests.Futureverse
{
    [TestFixture]
    public class FutureverseServiceTests
    {
        private IFutureverseService _futureverseService;
        private IWalletServiceInternal _walletServiceInternal;
        private IDisposable _verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            _verboseOutput = EmergenceLogger.VerboseOutput(true);
            EmergenceServiceProvider.Load();
            _futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            _walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _verboseOutput?.Dispose();
            EmergenceServiceProvider.Unload();
        }

        [Test]
        public void GetEnvironment_IsDevelopment()
        {
            using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Development);
            Assert.AreEqual(EmergenceEnvironment.Development, EmergenceSingleton.Instance.Environment);
        }

        [Test]
        public void GetEnvironment_IsStaging()
        {
            using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
            Assert.AreEqual(EmergenceEnvironment.Staging, EmergenceSingleton.Instance.Environment);
        }

        [Test]
        public void GetEnvironment_IsProduction()
        {
            using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Production);
            Assert.AreEqual(EmergenceEnvironment.Production, EmergenceSingleton.Instance.Environment);
        }
        
        [UnityTest]
        public IEnumerator GetAssetTreeAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                await _futureverseService.GetAssetTreeAsync("473", "7672:root:303204");
            });
        }

        [UnityTest]
        public IEnumerator GetArtmStatusAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                var artmStatusAsync = await _futureverseService.GetArtmStatusAsync("0x69c94ea3e0e7dea32d2d00813a64017dfbbd42dd18f5d56a12c907dccc7bb6d9");
                Assert.Pass("Passed with transaction status: " + artmStatusAsync);
            });
        }
        
                [UnityTest]
        public IEnumerator GetLinkedFuturepassAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var spoofedWallet = _walletServiceInternal.SpoofedWallet("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c");

                var response = await _futureverseService.GetLinkedFuturepassAsync();
                Assert.IsTrue(response.Successful);
                Assert.IsTrue(response.Code == ServiceResponseCode.Success);
                Assert.AreEqual(EmergenceSingleton.Instance.Configuration.Chain.ChainID + ":evm:0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", response.Result1.eoa);
            });
        }

        [UnityTest]
        public IEnumerator GetFuturepassInformationAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironment = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var spoofedWallet = _walletServiceInternal.SpoofedWallet("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c");

                var response = await _futureverseService.GetFuturepassInformationAsync("7668:root:0xffffffff0000000000000000000000000003b681");
                Assert.IsTrue(response.Successful);
                Assert.IsTrue(response.Code == ServiceResponseCode.Success);
                Assert.AreEqual("7668:root:0xffffffff0000000000000000000000000003b681", response.Result1.futurepass);
            });
        }
    }
}