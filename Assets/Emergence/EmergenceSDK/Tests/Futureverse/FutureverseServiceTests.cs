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
            EmergenceSingleton.Instance.RunInForcedEnvironment(EmergenceEnvironment.Development,
                () => { Assert.AreEqual(EmergenceEnvironment.Development, EmergenceSingleton.Instance.Environment); });
        }

        [Test]
        public void GetEnvironment_IsStaging()
        {
            EmergenceSingleton.Instance.RunInForcedEnvironment(EmergenceEnvironment.Staging,
                () => { Assert.AreEqual(EmergenceEnvironment.Staging, EmergenceSingleton.Instance.Environment); });
        }

        [Test]
        public void GetEnvironment_IsProduction()
        {
            EmergenceSingleton.Instance.RunInForcedEnvironment(EmergenceEnvironment.Production,
                () => { Assert.AreEqual(EmergenceEnvironment.Production, EmergenceSingleton.Instance.Environment); });
        }
    }
}