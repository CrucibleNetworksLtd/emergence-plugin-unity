using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Integrations.Futureverse.Internal;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Types;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace EmergenceSDK.Tests.Plugins.SimpleLibrary
{
    [TestFixture]
    public class SimpleLibraryTests
    {
        [Test]
        public void SimpleLibrary_Sum_Success()
        {
            Assert.AreEqual(2, SimpleLibraryTest.Add(1, 1));
            Assert.AreEqual(4, SimpleLibraryTest.Add(2, 2));
            Assert.AreEqual(6, SimpleLibraryTest.Add(3, 3));
        }

        [Test]
        public void SimpleLibrary_Concat_Success()
        {
            Assert.AreEqual("11", SimpleLibraryTest.Concat("1", "1"));
            Assert.AreEqual("22", SimpleLibraryTest.Concat("2", "2"));
            Assert.AreEqual("33", SimpleLibraryTest.Concat("3", "3"));
        }
    }
}