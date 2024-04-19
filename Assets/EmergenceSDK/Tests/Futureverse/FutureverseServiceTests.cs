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
    public class FutureverseServiceTests
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

        [Test]
        public void GetEnvironment_IsDevelopment()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            var futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            futureverseServiceInternal.RunInForcedEnvironment(FutureverseSingleton.Environment.Development,
                () => { Assert.AreEqual(FutureverseSingleton.Environment.Development, futureverseService.GetEnvironment()); });
        }

        [Test]
        public void GetEnvironment_IsStaging()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            var futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            futureverseServiceInternal.RunInForcedEnvironment(FutureverseSingleton.Environment.Staging,
                () => { Assert.AreEqual(FutureverseSingleton.Environment.Staging, futureverseService.GetEnvironment()); });
        }

        [Test]
        public void GetEnvironment_IsProduction()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            var futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            futureverseServiceInternal.RunInForcedEnvironment(FutureverseSingleton.Environment.Production,
                () => { Assert.AreEqual(FutureverseSingleton.Environment.Production, futureverseService.GetEnvironment()); });
        }
    }
}