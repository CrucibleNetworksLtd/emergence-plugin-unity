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
    public class AssetRegistryTests
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
        public void ParseAssetTree_ParsesCorrectly()
        {
            // ToDo: Redo this test or replace ParseGetAssetTreeJson with a simple call to SerializationHelper.Deserialize by restructuring the data
            string json = @"
            {
                ""data"": {
                    ""asset"": {
                        ""assetTree"": {
                            ""data"": {
                                ""@context"": {
                                    ""rdf"": ""http://www.w3.org/1999/02/22-rdf-syntax-ns#"",
                                    ""fv"": ""http://schema.futureverse.com#"",
                                    ""schema"": ""http://schema.org/"",
                                    ""path"": ""http://schema.futureverse.com/path#""
                                },
                                ""@graph"": [
                                    {
                                        ""@id"": ""did:fv-asset:7672:root:303204:473"",
                                        ""rdf:type"": {
                                            ""@id"": ""http://schema.futureverse.cloud/pb#bear""
                                        },
                                        ""http://schema.futureverse.com/fvp#sft_link_owner_0xffffffff00000000000000000000000000000524"": {
                                            ""@id"": ""did:fv-asset:7672:root:241764:0""
                                        },
                                        ""path:equippedWith_accessoryClothing"": {
                                            ""@id"": ""did:fv-asset:7672:root:241764:0""
                                        },
                                        ""path:equippedWith_accessoryMouth"": {
                                            ""@id"": ""did:fv-asset:7672:root:275556:3"",
                                            ""array"": [""sdfsdfsd"", ""ADASDASDA"", ""adasdada""],
                                            ""int"": 69,
                                            ""object"": {
                                                ""test"": [""sdfsdfsd"", ""ADASDASDA"", ""adasdada""]
                                            }
                                        }
                                    },
                                    {
                                        ""@id"": ""did:fv-asset:7672:root:275556:3"",
                                        ""rdf:type"": {
                                            ""@id"": ""http://schema.futureverse.com#None""
                                        }
                                    },
                                    {
                                        ""@id"": ""did:fv-asset:7672:root:241764:0"",
                                        ""rdf:type"": {
                                            ""@id"": ""http://schema.futureverse.com#None""
                                        }
                                    }
                                ]
                            }
                        }
                    }
                }
            }";

            var tree = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>().ParseGetAssetTreeJson(json);
            Assert.AreEqual(3, tree.Count);

            var firstPath = tree[0];
            Assert.AreEqual("did:fv-asset:7672:root:303204:473", firstPath.ID);
            Assert.AreEqual("http://schema.futureverse.cloud/pb#bear", firstPath.RdfType);
            Assert.AreEqual(3, firstPath.Objects.Count);
            var firstObject =
                firstPath.Objects[
                    "http://schema.futureverse.com/fvp#sft_link_owner_0xffffffff00000000000000000000000000000524"];
            Assert.NotNull(firstObject);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", firstObject.ID);
            Assert.IsEmpty(firstObject.AdditionalData);
            var secondObject = firstPath.Objects["path:equippedWith_accessoryClothing"];
            Assert.NotNull(secondObject);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", secondObject.ID);
            Assert.IsEmpty(secondObject.AdditionalData);
            var thirdObject = firstPath.Objects["path:equippedWith_accessoryMouth"];
            Assert.NotNull(thirdObject);
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", thirdObject.ID);
            Assert.AreEqual(3, thirdObject.AdditionalData.Count);
            var thirdObjectAdditionalArray = thirdObject.AdditionalData["array"];
            Assert.IsInstanceOf<JArray>(thirdObjectAdditionalArray);
            Assert.AreEqual(@"[""sdfsdfsd"",""ADASDASDA"",""adasdada""]",
                thirdObjectAdditionalArray.ToString(Newtonsoft.Json.Formatting.None));
            var thirdObjectAdditionalInt = thirdObject.AdditionalData["int"];
            Assert.IsInstanceOf<JValue>(thirdObjectAdditionalInt);
            Assert.AreEqual("69",
                thirdObjectAdditionalInt.ToString(Newtonsoft.Json.Formatting.None));
            var thirdObjectAdditionalObject = thirdObject.AdditionalData["object"];
            Assert.IsInstanceOf<JObject>(thirdObjectAdditionalObject);
            Assert.AreEqual(@"{""test"":[""sdfsdfsd"",""ADASDASDA"",""adasdada""]}",
                thirdObjectAdditionalObject.ToString(Newtonsoft.Json.Formatting.None));

            var secondPath = tree[1];
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", secondPath.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", secondPath.RdfType);
            Assert.IsEmpty(secondPath.Objects);

            var thirdPath = tree[2];
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", thirdPath.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", thirdPath.RdfType);
            Assert.IsEmpty(thirdPath.Objects);
        }

        [UnityTest]
        public IEnumerator GetAssetTreeAsync_PassesWithoutExceptions()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            return UniTask.ToCoroutine(async () =>
            {
                await FutureverseSingleton.Instance.RunInForcedEnvironmentAsync(EmergenceEnvironment.Development, async () =>
                {
                    await futureverseService.GetAssetTreeAsync("473", "7672:root:303204");
                });
            });
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

            var result = ArtmBuilder.GenerateArtm("Message", new List<FutureverseArtmOperation>
            {
                new(FutureverseArtmOperationType.CreateLink, "slot", "linkA", "linkB"),
                new(FutureverseArtmOperationType.DeleteLink, "slot", "linkA", "linkB"),
            }, "Address", 123456789);

            Assert.AreEqual(expected, result);
        }

        [UnityTest]
        public IEnumerator GetArtmStatusAsync_PassesWithoutExceptions()
        {
            var futureverseService = EmergenceServiceProvider.GetService<IFutureverseService>();
            return UniTask.ToCoroutine(async () =>
            {
                await FutureverseSingleton.Instance.RunInForcedEnvironmentAsync(EmergenceEnvironment.Staging, async () =>
                {
                    var artmStatusAsync = await futureverseService.GetArtmStatusAsync("0x69c94ea3e0e7dea32d2d00813a64017dfbbd42dd18f5d56a12c907dccc7bb6d9");
                    Assert.Pass("Passed with transaction status: " + artmStatusAsync);
                });
            });
        }
    }
}