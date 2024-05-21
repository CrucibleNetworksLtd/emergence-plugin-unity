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
    [TestFixture]
    public class AssetTreeTests
    {
        private IDisposable verboseOutput;
        private List<AssetTreePath> parsedTree;
        private AssetTreePath firstElement;
        private AssetTreePath secondElement;
        private AssetTreePath thirdElement;
        private AssetTreePath.Object firstObjectOfFirstElement;
        private AssetTreePath.Object secondObjectOfFirstElement;
        private AssetTreePath.Object thirdObjectOfFirstElement;
        private JToken additionalArray;
        private JToken additionalInt;
        private JToken additionalObject;
        private IFutureverseServiceInternal futureverseServiceInternal;

        [OneTimeSetUp]
        public void Setup()
        {
            EmergenceServiceProvider.Load(ServiceProfile.Futureverse);
            futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            parsedTree = futureverseServiceInternal.DeserializeGetAssetTreeResponseJson(Json);
            verboseOutput = EmergenceLogger.VerboseOutput(true);

            firstElement = parsedTree[0];
            secondElement = parsedTree[1];
            thirdElement = parsedTree[2];

            firstObjectOfFirstElement = firstElement.Objects["http://schema.futureverse.com/fvp#sft_link_owner_0xffffffff00000000000000000000000000000524"];
            secondObjectOfFirstElement = firstElement.Objects["Element:equippedWith_accessoryClothing"];
            thirdObjectOfFirstElement = firstElement.Objects["Element:equippedWith_accessoryMouth"];

            additionalArray = thirdObjectOfFirstElement.AdditionalData["array"];
            additionalInt = thirdObjectOfFirstElement.AdditionalData["int"];
            additionalObject = thirdObjectOfFirstElement.AdditionalData["object"];
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
        public void CheckFirstElement_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:303204:473", firstElement.ID);
            Assert.AreEqual("http://schema.futureverse.cloud/pb#bear", firstElement.RdfType);
            Assert.IsNotEmpty(firstElement.Objects);
            Assert.IsTrue(firstElement.Objects.ContainsKey("rdf:type"));
        }

        [Test]
        public void CheckFirstObjectOfFirstElement_IsCorrect()
        {
            Assert.NotNull(firstObjectOfFirstElement);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", firstObjectOfFirstElement.ID);
            Assert.IsEmpty(firstObjectOfFirstElement.AdditionalData);
        }

        [Test]
        public void CheckSecondObjectOfFirstElement_IsCorrect()
        {
            Assert.NotNull(secondObjectOfFirstElement);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", secondObjectOfFirstElement.ID);
            Assert.IsEmpty(secondObjectOfFirstElement.AdditionalData);
        }

        [Test]
        public void CheckThirdObjectOfFirstElement_IsCorrect()
        {
            Assert.NotNull(thirdObjectOfFirstElement);
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", thirdObjectOfFirstElement.ID);
            Assert.AreEqual(3, thirdObjectOfFirstElement.AdditionalData.Count);
        }

        [Test]
        public void CheckThirdObjectOfFirstElementAdditionalArray_IsCorrect()
        {
            Assert.IsInstanceOf<JArray>(additionalArray);
            Assert.AreEqual(@"[""sdfsdfsd"",""ADASDASDA"",""adasdada""]", additionalArray.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstElementAdditionalInt_IsCorrect()
        {
            Assert.IsInstanceOf<JValue>(additionalInt);
            Assert.AreEqual("69", additionalInt.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstElementAdditionalObject_IsCorrect()
        {
            Assert.IsInstanceOf<JObject>(additionalObject);
            Assert.AreEqual(@"{""test"":[""sdfsdfsd"",""ADASDASDA"",""adasdada""]}", additionalObject.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckSecondElement_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", secondElement.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", secondElement.RdfType);
            Assert.IsNotEmpty(secondElement.Objects);
            Assert.IsTrue(secondElement.Objects.ContainsKey("rdf:type"));
        }

        [Test]
        public void CheckThirdElement_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", thirdElement.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", thirdElement.RdfType);
            Assert.IsNotEmpty(thirdElement.Objects);
            Assert.IsTrue(secondElement.Objects.ContainsKey("rdf:type"));
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
                                ""Element"": ""http://schema.futureverse.com/Element#""
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
                                    ""Element:equippedWith_accessoryClothing"": {
                                        ""@id"": ""did:fv-asset:7672:root:241764:0""
                                    },
                                    ""Element:equippedWith_accessoryMouth"": {
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
