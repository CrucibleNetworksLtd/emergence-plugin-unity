using System;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Futureverse.Internal;
using EmergenceSDK.Runtime.Futureverse.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using NUnit.Framework;

namespace EmergenceSDK.Tests.UnitTests.Futureverse
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