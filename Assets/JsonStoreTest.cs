
using System;
using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.Internal.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace EmergenceSDK
{
    // ToDo: Turn into unit test
    public class JsonStoreTest : MonoBehaviour
    {
        [SerializationHelper.StoreOriginalJTokens]
        class NestedTestObject
        {
            public class TestClass
            {
                public class SimpleClass
                {
                    public int a;
                    public int b;
                    public int c;

                    [SerializationHelper.OriginalJToken]
                    public JToken OriginalToken;
                }
                
                public class TestNestedClass
                {
                    public class TestNestedClassTwo
                    {
                        public string a;
                        public string b;
                        public List<SimpleClass> testClassList;
                        public Dictionary<string, SimpleClass> testClassDictionary;
                        public TestNestedClassTwo testNestedClass;
                        [SerializationHelper.OriginalJToken]
                        public JToken OriginalToken;
                    }
                    
                    public string a;
                    public string b;
                    public List<SimpleClass> testClassList;
                    public Dictionary<string, SimpleClass> testClassDictionary;
                    public TestNestedClassTwo testNestedClass;
                    [SerializationHelper.OriginalJToken]
                    public JToken OriginalToken;
                }
            
                public string a;
                public string b;
                public List<SimpleClass> testClassList;
                public Dictionary<string, SimpleClass> testClassDictionary;
                public TestNestedClass testNestedClass;
                [SerializationHelper.OriginalJToken]
                public JToken OriginalToken;
            }
            
            public TestClass testClass;
            [SerializationHelper.OriginalJToken]
            public JToken OriginalToken;
        }
        
        [SerializationHelper.StoreOriginalJTokens]
        class AccessibilityTestObject
        {
            public class TestClass
            {
                public string description;
                [JsonProperty("should_show")]
                public bool shouldShow;
                
                [SerializationHelper.OriginalJToken]
                public JToken OriginalToken;
            }
            
            public TestClass a;
            private TestClass b;
            [JsonProperty]
            private TestClass c;
            public TestClass d { get; set; }
            private TestClass e { get; set; }
            [JsonProperty]
            private TestClass f { get; set; }
            [JsonProperty]
            public TestClass g { get; private set; }
            public TestClass h { get; private set; }
            [JsonProperty]
            public TestClass i { get; }
            public TestClass j { get; }
            [JsonIgnore]
            public TestClass k;
            [JsonIgnore]
            public TestClass l { get; set; }
            
            [SerializationHelper.OriginalJToken]
            public JToken OriginalToken;
        }
        
        [SerializationHelper.StoreOriginalJTokens]
        class CustomPropertyNameTestObject
        {
            public class TestClass
            {
                public string description;
                
                [SerializationHelper.OriginalJToken]
                public JToken OriginalToken;
            }
            
            public TestClass actual_property;
            [JsonProperty("custom_property")]
            public TestClass customProperty;
            
            [SerializationHelper.OriginalJToken]
            public JToken OriginalToken;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            string accessibilityJsonTestObject = @"{
                ""a"": {
                    ""description"": ""Public Field"",
                    ""should_show"": true
                },
                ""b"": {
                    ""description"": ""Private Field"",
                    ""should_show"": false
                },
                ""c"": {
                    ""description"": ""Private Field w/ JsonProperty Attribute"",
                    ""should_show"": true
                },
                ""d"": {
                    ""description"": ""Public Property"",
                    ""should_show"": true
                },
                ""e"": {
                    ""description"": ""Private Property"",
                    ""should_show"": false
                },
                ""f"": {
                    ""description"": ""Private Property w/ JsonProperty Attribute"",
                    ""should_show"": true
                },
                ""g"": {
                    ""description"": ""Public Property w/ Private Setter & JsonProperty Attribute"",
                    ""should_show"": true
                },
                ""h"": {
                    ""description"": ""Public Property w/ Private Setter"",
                    ""should_show"": false
                },
                ""i"": {
                    ""description"": ""Public Property w/o Setter w/ JsonProperty Attribute "",
                    ""should_show"": false
                },
                ""j"": {
                    ""description"": ""Public Property w/o Setter "",
                    ""should_show"": false
                },
                ""k"": {
                    ""description"": ""Public Field w/ JsonIgnore Attribute "",
                    ""should_show"": false
                },
                ""l"": {
                    ""description"": ""Public Property w/ JsonIgnore Attribute "",
                    ""should_show"": false
                }
            }";
            
            string nestingTestJson = @"{
                ""testClass"": {
                    ""a"": ""1"",
                    ""b"": ""2"",
                    ""testClassList"": [
                        {""a"": 1, ""b"": 2, ""c"": 3},
                        {""a"": 4, ""b"": 5, ""c"": 6}
                    ],
                    ""testClassDictionary"": {
                        ""a"": {""a"": 7, ""b"": 8, ""c"": 9},
                        ""b"": {""a"": 10, ""b"": 11, ""c"": 12},
                        ""c"": {""a"": 13, ""b"": 14, ""c"": 15}
                    },
                    ""testNestedClass"": {
                        ""c"": ""1"",
                        ""d"": ""2"",
                        ""testClassList"": [
                            {""a"": 1, ""b"": 2, ""c"": 3},
                            {""a"": 4, ""b"": 5, ""c"": 6}
                        ],
                        ""testClassDictionary"": {
                            ""a"": {""a"": 7, ""b"": 8, ""c"": 9},
                            ""b"": {""a"": 10, ""b"": 11, ""c"": 12},
                            ""c"": {""a"": 13, ""b"": 14, ""c"": 15}
                        },
                        ""testNestedClass"": {
                            ""c"": ""1"",
                            ""d"": ""2"",
                            ""testClassList"": [
                                {""a"": 1, ""b"": 2, ""c"": 3},
                                {""a"": 4, ""b"": 5, ""c"": 6}
                            ],
                            ""testClassDictionary"": {
                                ""a"": {""a"": 7, ""b"": 8, ""c"": 9},
                                ""b"": {""a"": 10, ""b"": 11, ""c"": 12},
                                ""c"": {""a"": 13, ""b"": 14, ""c"": 15}
                            }
                        }
                    }
                }
            }";
            
            string customPropertyNameTestJson = @"{
                ""actual_property"": {
                    ""description"": ""Property matching C# name"",
                },
                ""custom_property"": {
                    ""description"": ""Property not matching C# name but specified with JsonProperty attribute"",
                    ""should_show"": false
                }
            }";

            var testObject = SerializationHelper.Deserialize<NestedTestObject>(nestingTestJson);
            var accessibilityTestObject = SerializationHelper.Deserialize<AccessibilityTestObject>(accessibilityJsonTestObject);
            var customPropertyNameTestObject = SerializationHelper.Deserialize<CustomPropertyNameTestObject>(customPropertyNameTestJson);
        }
    }
}
