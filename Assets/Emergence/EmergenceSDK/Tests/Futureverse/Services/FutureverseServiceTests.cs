using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Services;
using EmergenceSDK.Types;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EmergenceSDK.Tests.Futureverse.Services
{
    [TestFixture]
    public class FutureverseServiceTests
    {
        private IFutureverseService _futureverseService;
        private IWalletServiceDevelopmentOnly _walletServiceDevelopmentOnly;
        private IDisposable _verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            _verboseOutput = EmergenceLogger.VerboseOutput(true);
            EmergenceServiceProvider.Load();
            _futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            _walletServiceDevelopmentOnly = EmergenceServiceProvider.GetService<IWalletServiceDevelopmentOnly>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _verboseOutput?.Dispose();
            EmergenceServiceProvider.Unload();
        }
        
        [UnityTest][Obsolete]
        public IEnumerator GetAssetTreeAsyncLegacy_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironmentEmergence = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var forcedEnvironment = FutureverseSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                await _futureverseService.GetAssetTreeAsyncLegacy("473", "7672:root:303204");
            });
        }

        [UnityTest]
        public IEnumerator GetArtmStatusAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironmentEmergence = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var forcedEnvironment = FutureverseSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                var artmStatusAsync = await _futureverseService.GetArtmStatusAsync("0x69c94ea3e0e7dea32d2d00813a64017dfbbd42dd18f5d56a12c907dccc7bb6d9");
                Assert.Pass("Passed with transaction status: " + artmStatusAsync);
            });
        }
        
        [UnityTest]
        public IEnumerator GetLinkedFuturepassAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironmentEmergence = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var forcedEnvironment = FutureverseSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var spoofedWallet = _walletServiceDevelopmentOnly.SpoofedWallet("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c");

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
                using var forcedEnvironmentEmergence = EmergenceSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var forcedEnvironment = FutureverseSingleton.Instance.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var spoofedWallet = _walletServiceDevelopmentOnly.SpoofedWallet("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c");

                var response = await _futureverseService.GetFuturepassInformationAsync("7668:root:0xffffffff0000000000000000000000000003b681");
                Assert.IsTrue(response.Successful);
                Assert.IsTrue(response.Code == ServiceResponseCode.Success);
                Assert.AreEqual("7668:root:0xffffffff0000000000000000000000000003b681", response.Result1.futurepass);
            });
        }
    }
}