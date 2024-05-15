using System;
using System.Collections.Generic;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Internal.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EmergenceSDK.Tests.Futureverse
{
    [TestFixture]
    public class AssetTreeTests
    {
        private IDisposable _verboseOutput;
        private List<AssetTreePath> _parsedTree;
        private AssetTreePath _firstElement;
        private AssetTreePath _secondElement;
        private AssetTreePath _thirdElement;
        private AssetTreePath.Object _firstObjectOfFirstElement;
        private AssetTreePath.Object _secondObjectOfFirstElement;
        private AssetTreePath.Object _thirdObjectOfFirstElement;
        private JToken _additionalArray;
        private JToken _additionalInt;
        private JToken _additionalObject;
        private IFutureverseServiceInternal _futureverseServiceInternal;

        [OneTimeSetUp]
        public void Setup()
        {
            EmergenceServiceProvider.Load();
            _futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            _parsedTree = _futureverseServiceInternal.DeserializeGetAssetTreeResponseJson(Json);
            _verboseOutput = EmergenceLogger.VerboseOutput(true);

            _firstElement = _parsedTree[0];
            _secondElement = _parsedTree[1];
            _thirdElement = _parsedTree[2];

            _firstObjectOfFirstElement = _firstElement.Objects["http://schema.futureverse.com/fvp#sft_link_owner_0xffffffff00000000000000000000000000000524"];
            _secondObjectOfFirstElement = _firstElement.Objects["Element:equippedWith_accessoryClothing"];
            _thirdObjectOfFirstElement = _firstElement.Objects["Element:equippedWith_accessoryMouth"];

            _additionalArray = _thirdObjectOfFirstElement.AdditionalData["array"];
            _additionalInt = _thirdObjectOfFirstElement.AdditionalData["int"];
            _additionalObject = _thirdObjectOfFirstElement.AdditionalData["object"];
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _verboseOutput?.Dispose();
        }

        [Test]
        public void CheckTreeCount_IsThree()
        {
            Assert.AreEqual(3, _parsedTree.Count);
        }

        [Test]
        public void CheckFirstElement_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:303204:473", _firstElement.ID);
            Assert.AreEqual("http://schema.futureverse.cloud/pb#bear", _firstElement.RdfType);
            Assert.IsNotEmpty(_firstElement.Objects);
            Assert.IsTrue(_firstElement.Objects.ContainsKey("rdf:type"));
        }

        [Test]
        public void CheckFirstObjectOfFirstElement_IsCorrect()
        {
            Assert.NotNull(_firstObjectOfFirstElement);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", _firstObjectOfFirstElement.ID);
            Assert.IsEmpty(_firstObjectOfFirstElement.AdditionalData);
        }

        [Test]
        public void CheckSecondObjectOfFirstElement_IsCorrect()
        {
            Assert.NotNull(_secondObjectOfFirstElement);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", _secondObjectOfFirstElement.ID);
            Assert.IsEmpty(_secondObjectOfFirstElement.AdditionalData);
        }

        [Test]
        public void CheckThirdObjectOfFirstElement_IsCorrect()
        {
            Assert.NotNull(_thirdObjectOfFirstElement);
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", _thirdObjectOfFirstElement.ID);
            Assert.AreEqual(3, _thirdObjectOfFirstElement.AdditionalData.Count);
        }

        [Test]
        public void CheckThirdObjectOfFirstElementAdditionalArray_IsCorrect()
        {
            Assert.IsInstanceOf<JArray>(_additionalArray);
            Assert.AreEqual(@"[""sdfsdfsd"",""ADASDASDA"",""adasdada""]", _additionalArray.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstElementAdditionalInt_IsCorrect()
        {
            Assert.IsInstanceOf<JValue>(_additionalInt);
            Assert.AreEqual("69", _additionalInt.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstElementAdditionalObject_IsCorrect()
        {
            Assert.IsInstanceOf<JObject>(_additionalObject);
            Assert.AreEqual(@"{""test"":[""sdfsdfsd"",""ADASDASDA"",""adasdada""]}", _additionalObject.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckSecondElement_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", _secondElement.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", _secondElement.RdfType);
            Assert.IsNotEmpty(_secondElement.Objects);
            Assert.IsTrue(_secondElement.Objects.ContainsKey("rdf:type"));
        }

        [Test]
        public void CheckThirdElement_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", _thirdElement.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", _thirdElement.RdfType);
            Assert.IsNotEmpty(_thirdElement.Objects);
            Assert.IsTrue(_secondElement.Objects.ContainsKey("rdf:type"));
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
