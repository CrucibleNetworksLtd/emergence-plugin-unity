using System;
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
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmergenceSDK.Tests.Futureverse
{
    [TestFixture]
    public class AssetRegistryTests
    {
        private IDisposable verboseOutput;

        [OneTimeSetUp]
        public void Setup()
        {
            verboseOutput = EmergenceLogger.VerboseOutput(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            verboseOutput?.Dispose();
        }

        [Test]
        public void GenerateArtm_GeneratesCorrectly()
        {
            #region ExpectedResult

            const string expected =
                "Asset Registry transaction\n" +
                "\n" +
                "Message\n" +
                "\n" +
                "Operations:\n" +
                "\n" +
                "asset-link create\n" +
                "- slot\n" +
                "- linkA\n" +
                "- linkB\n" +
                "end\n" +
                "\n" +
                "asset-link delete\n" +
                "- slot\n" +
                "- linkA\n" +
                "- linkB\n" +
                "end\n" +
                "\n" +
                "Operations END\n" +
                "\n" +
                "Address: Address\n" +
                "Nonce: 123456789";

            #endregion

            var result = ArtmBuilder.GenerateArtm("Message", new List<ArtmOperation>
            {
                new(ArtmOperationType.CreateLink, "slot", "linkA", "linkB"),
                new(ArtmOperationType.DeleteLink, "slot", "linkA", "linkB"),
            }, "Address", 123456789);

            Assert.AreEqual(expected, result);
        }
    }
}