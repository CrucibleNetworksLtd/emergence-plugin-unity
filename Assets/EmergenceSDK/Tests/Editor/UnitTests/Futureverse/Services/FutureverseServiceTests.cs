using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EmergenceSDK.Tests.UnitTests.Futureverse.Services
{
    [TestFixture]
    public class FutureverseServiceTests
    {
        private IFutureverseService futureverseService;
        private FutureverseInventoryService futureverseInventoryService;
        private IWalletServiceDevelopmentOnly walletServiceDevelopmentOnly;
        private IDisposable verboseOutput;
        [OneTimeSetUp]
        public void Setup()
        {
            verboseOutput = EmergenceLogger.VerboseOutput(true);
            EmergenceServiceProvider.Load(ServiceProfile.Futureverse);
            futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            futureverseInventoryService = EmergenceServiceProvider.GetService<IInventoryService>() as FutureverseInventoryService;
            walletServiceDevelopmentOnly = EmergenceServiceProvider.GetService<IWalletServiceDevelopmentOnly>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            verboseOutput?.Dispose();
            EmergenceServiceProvider.Unload();
        }
        

        [UnityTest]
        public IEnumerator GetArtmStatusAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironment = InternalFutureverseSingleton.ForcedEnvironment(EmergenceEnvironment.Staging);
                var artmStatusAsync = await futureverseService.GetArtmStatusAsync("0x69c94ea3e0e7dea32d2d00813a64017dfbbd42dd18f5d56a12c907dccc7bb6d9");
                Assert.Pass("Passed with transaction status: " + artmStatusAsync);
            });
        }
        
        [UnityTest]
        public IEnumerator GetLinkedFuturepassAsync_PassesWithoutExceptions()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var forcedEnvironment = InternalFutureverseSingleton.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var spoofedWallet = walletServiceDevelopmentOnly.SpoofedWallet("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c");

                var response = await futureverseService.GetLinkedFuturepassAsync();
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
                using var forcedEnvironment = InternalFutureverseSingleton.ForcedEnvironment(EmergenceEnvironment.Staging);
                using var spoofedWallet = walletServiceDevelopmentOnly.SpoofedWallet("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c");

                var response = await futureverseService.GetFuturepassInformationAsync("7668:root:0xffffffff0000000000000000000000000003b681");
                Assert.IsTrue(response.Successful);
                Assert.IsTrue(response.Code == ServiceResponseCode.Success);
                Assert.AreEqual("7668:root:0xffffffff0000000000000000000000000003b681", response.Result1.futurepass);
            });
        }
    }
}