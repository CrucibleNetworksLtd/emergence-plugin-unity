using System;
using System.Collections.Generic;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Internal.Utils;
using EmergenceSDK.Types;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EmergenceSDK.Tests.Futureverse
{
    [TestFixture][Obsolete]
    public class AssetTreeLegacyTests
    {
        private IDisposable verboseOutput;
        private List<AssetTreePathLegacy> parsedTree;
        private AssetTreePathLegacy firstPath;
        private AssetTreePathLegacy secondPath;
        private AssetTreePathLegacy thirdPath;
        private AssetTreeObjectLegacy firstObjectOfFirstPath;
        private AssetTreeObjectLegacy secondObjectOfFirstPath;
        private AssetTreeObjectLegacy thirdObjectOfFirstPath;
        private JToken additionalArray;
        private JToken additionalInt;
        private JToken additionalObject;
        private IFutureverseServiceInternal futureverseServiceInternal;

        [OneTimeSetUp]
        public void Setup()
        {
            EmergenceServiceProvider.Load(ServiceProfile.Futureverse);
            futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            parsedTree = futureverseServiceInternal.ParseGetAssetTreeResponseJsonLegacy(Json);
            verboseOutput = EmergenceLogger.VerboseOutput(true);

            firstPath = parsedTree[0];
            secondPath = parsedTree[1];
            thirdPath = parsedTree[2];

            firstObjectOfFirstPath = firstPath.Objects["http://schema.futureverse.com/fvp#sft_link_owner_0xffffffff00000000000000000000000000000524"];
            secondObjectOfFirstPath = firstPath.Objects["path:equippedWith_accessoryClothing"];
            thirdObjectOfFirstPath = firstPath.Objects["path:equippedWith_accessoryMouth"];

            additionalArray = thirdObjectOfFirstPath.AdditionalData["array"];
            additionalInt = thirdObjectOfFirstPath.AdditionalData["int"];
            additionalObject = thirdObjectOfFirstPath.AdditionalData["object"];
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            verboseOutput?.Dispose();
        }

        [Test]
        public void CheckTreeCount_IsThree()
        {
            Assert.AreEqual(3, parsedTree.Count);
        }

        [Test]
        public void CheckFirstPath_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:303204:473", firstPath.ID);
            Assert.AreEqual("http://schema.futureverse.cloud/pb#bear", firstPath.RdfType);
        }

        [Test]
        public void CheckFirstObjectOfFirstPath_IsCorrect()
        {
            Assert.NotNull(firstObjectOfFirstPath);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", firstObjectOfFirstPath.ID);
            Assert.IsEmpty(firstObjectOfFirstPath.AdditionalData);
        }

        [Test]
        public void CheckSecondObjectOfFirstPath_IsCorrect()
        {
            Assert.NotNull(secondObjectOfFirstPath);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", secondObjectOfFirstPath.ID);
            Assert.IsEmpty(secondObjectOfFirstPath.AdditionalData);
        }

        [Test]
        public void CheckThirdObjectOfFirstPath_IsCorrect()
        {
            Assert.NotNull(thirdObjectOfFirstPath);
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", thirdObjectOfFirstPath.ID);
            Assert.AreEqual(3, thirdObjectOfFirstPath.AdditionalData.Count);
        }

        [Test]
        public void CheckThirdObjectOfFirstPathAdditionalArray_IsCorrect()
        {
            Assert.IsInstanceOf<JArray>(additionalArray);
            Assert.AreEqual(@"[""sdfsdfsd"",""ADASDASDA"",""adasdada""]", additionalArray.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstPathAdditionalInt_IsCorrect()
        {
            Assert.IsInstanceOf<JValue>(additionalInt);
            Assert.AreEqual("69", additionalInt.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstPathAdditionalObject_IsCorrect()
        {
            Assert.IsInstanceOf<JObject>(additionalObject);
            Assert.AreEqual(@"{""test"":[""sdfsdfsd"",""ADASDASDA"",""adasdada""]}", additionalObject.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckSecondPath_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", secondPath.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", secondPath.RdfType);
            Assert.IsEmpty(secondPath.Objects);
        }

        [Test]
        public void CheckThirdPath_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", thirdPath.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", thirdPath.RdfType);
            Assert.IsEmpty(thirdPath.Objects);
        }
        
        const string Json = @"
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
    }
}
