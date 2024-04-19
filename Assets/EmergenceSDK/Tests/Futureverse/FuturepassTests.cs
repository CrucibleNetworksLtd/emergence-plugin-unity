using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse;
using EmergenceSDK.Integrations.Futureverse.Internal;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Types;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmergenceSDK.Tests.Futureverse
{
    [TestFixture]
    public class FuturepassTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            EmergenceServiceProvider.Load();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            EmergenceServiceProvider.Unload();
        }

        [UnityTest]
        public IEnumerator GetLinkedFuturepassAsync_PassesWithoutExceptions()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            var futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            var walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
            return UniTask.ToCoroutine(async () =>
            {
                await futureverseServiceInternal.RunInForcedEnvironmentAsync(FutureverseSingleton.Environment.Staging,
                    async () =>
                    {
                        await walletServiceInternal.RunWithSpoofedWalletAddressAsync("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c", async () =>
                        {
                            var response = await futureverseService.GetLinkedFuturepassAsync();
                            Assert.IsTrue(response.Success);
                            Assert.IsTrue(response.Code == ServiceResponseCode.Success);
                            Assert.AreEqual("1:evm:0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", response.Result.eoa);
                        });
                    });
            });
        }

        [UnityTest]
        public IEnumerator GetFuturepassInformationAsync_PassesWithoutExceptions()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            var futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            var walletServiceInternal = EmergenceServiceProvider.GetService<IWalletServiceInternal>();
            return UniTask.ToCoroutine(async () =>
            {
                await futureverseServiceInternal.RunInForcedEnvironmentAsync(FutureverseSingleton.Environment.Staging,
                    async () =>
                    {
                        await walletServiceInternal.RunWithSpoofedWalletAddressAsync("0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", "0xeC6F83b0d5Ada27c68FC64Cf63f1Db56CB11A37c", async () =>
                        {
                            var response = await futureverseService.GetFuturepassInformationAsync("7668:root:0xffffffff0000000000000000000000000003b681");
                            Assert.IsTrue(response.Success);
                            Assert.IsTrue(response.Code == ServiceResponseCode.Success);
                            Assert.AreEqual("7668:root:0xffffffff0000000000000000000000000003b681", response.Result.futurepass);
                            Assert.AreEqual("1:evm:0xec6f83b0d5ada27c68fc64cf63f1db56cb11a37c", response.Result.ownerEoa);
                        });
                    });
            });
        }
    }
}