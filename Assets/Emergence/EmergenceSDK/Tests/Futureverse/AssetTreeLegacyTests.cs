using System;
using System.Collections.Generic;
using EmergenceSDK.Integrations.Futureverse.Internal.Services;
using EmergenceSDK.Integrations.Futureverse.Types;
using EmergenceSDK.Internal.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EmergenceSDK.Tests.Futureverse
{
    [TestFixture][Obsolete]
    public class AssetTreeLegacyTests
    {
        private IDisposable _verboseOutput;
        private List<AssetTreePathLegacy> _parsedTree;
        private AssetTreePathLegacy _firstPath;
        private AssetTreePathLegacy _secondPath;
        private AssetTreePathLegacy _thirdPath;
        private AssetTreeObjectLegacy _firstObjectOfFirstPath;
        private AssetTreeObjectLegacy _secondObjectOfFirstPath;
        private AssetTreeObjectLegacy _thirdObjectOfFirstPath;
        private JToken _additionalArray;
        private JToken _additionalInt;
        private JToken _additionalObject;
        private IFutureverseServiceInternal _futureverseServiceInternal;

        [OneTimeSetUp]
        public void Setup()
        {
            EmergenceServiceProvider.Load();
            _futureverseServiceInternal = EmergenceServiceProvider.GetService<IFutureverseServiceInternal>();
            _parsedTree = _futureverseServiceInternal.ParseGetAssetTreeResponseJsonLegacy(Json);
            _verboseOutput = EmergenceLogger.VerboseOutput(true);

            _firstPath = _parsedTree[0];
            _secondPath = _parsedTree[1];
            _thirdPath = _parsedTree[2];

            _firstObjectOfFirstPath = _firstPath.Objects["http://schema.futureverse.com/fvp#sft_link_owner_0xffffffff00000000000000000000000000000524"];
            _secondObjectOfFirstPath = _firstPath.Objects["path:equippedWith_accessoryClothing"];
            _thirdObjectOfFirstPath = _firstPath.Objects["path:equippedWith_accessoryMouth"];

            _additionalArray = _thirdObjectOfFirstPath.AdditionalData["array"];
            _additionalInt = _thirdObjectOfFirstPath.AdditionalData["int"];
            _additionalObject = _thirdObjectOfFirstPath.AdditionalData["object"];
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
        public void CheckFirstPath_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:303204:473", _firstPath.ID);
            Assert.AreEqual("http://schema.futureverse.cloud/pb#bear", _firstPath.RdfType);
        }

        [Test]
        public void CheckFirstObjectOfFirstPath_IsCorrect()
        {
            Assert.NotNull(_firstObjectOfFirstPath);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", _firstObjectOfFirstPath.ID);
            Assert.IsEmpty(_firstObjectOfFirstPath.AdditionalData);
        }

        [Test]
        public void CheckSecondObjectOfFirstPath_IsCorrect()
        {
            Assert.NotNull(_secondObjectOfFirstPath);
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", _secondObjectOfFirstPath.ID);
            Assert.IsEmpty(_secondObjectOfFirstPath.AdditionalData);
        }

        [Test]
        public void CheckThirdObjectOfFirstPath_IsCorrect()
        {
            Assert.NotNull(_thirdObjectOfFirstPath);
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", _thirdObjectOfFirstPath.ID);
            Assert.AreEqual(3, _thirdObjectOfFirstPath.AdditionalData.Count);
        }

        [Test]
        public void CheckThirdObjectOfFirstPathAdditionalArray_IsCorrect()
        {
            Assert.IsInstanceOf<JArray>(_additionalArray);
            Assert.AreEqual(@"[""sdfsdfsd"",""ADASDASDA"",""adasdada""]", _additionalArray.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstPathAdditionalInt_IsCorrect()
        {
            Assert.IsInstanceOf<JValue>(_additionalInt);
            Assert.AreEqual("69", _additionalInt.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckThirdObjectOfFirstPathAdditionalObject_IsCorrect()
        {
            Assert.IsInstanceOf<JObject>(_additionalObject);
            Assert.AreEqual(@"{""test"":[""sdfsdfsd"",""ADASDASDA"",""adasdada""]}", _additionalObject.ToString(Newtonsoft.Json.Formatting.None));
        }

        [Test]
        public void CheckSecondPath_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:275556:3", _secondPath.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", _secondPath.RdfType);
            Assert.IsEmpty(_secondPath.Objects);
        }

        [Test]
        public void CheckThirdPath_IsCorrect()
        {
            Assert.AreEqual("did:fv-asset:7672:root:241764:0", _thirdPath.ID);
            Assert.AreEqual("http://schema.futureverse.com#None", _thirdPath.RdfType);
            Assert.IsEmpty(_thirdPath.Objects);
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
