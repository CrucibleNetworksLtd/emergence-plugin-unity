using System;
using Newtonsoft.Json.Linq;
using System.Text;
using EmergenceSDK.Internal.Utils;

namespace ABIToDotNet
{
    public class ABIToCSharp
    {
        private readonly string className;
        private const string Indent = "    ";

        public string CSharpClass => cSharpClass;
        private string cSharpClass;
        private readonly ContractInfo contractInfo;

        public ABIToCSharp(ContractInfo contractInfo)
        {
            this.contractInfo = contractInfo ?? throw new ArgumentNullException(nameof(contractInfo));
            className = $"{this.contractInfo.Network}_{contractInfo.MethodName}";
            cSharpClass = GenerateCSharpClassFromABI();
        }

        private string GenerateCSharpClassFromABI()
        {
            // Parse the ABI string to a JArray
            JArray abiArray = JArray.Parse(contractInfo.ABI);

            // Create a StringBuilder to store the generated class
            StringBuilder sb = new StringBuilder();

            // Generate the class definition
            AppendLineWithIndent(sb, "// This is a generated class, be aware that any changes made to it will be overwritten");
            AppendLineWithIndent(sb, $"// Generated from contract {contractInfo.ContractAddress} on network {contractInfo.Network}");
            sb.AppendLine($"using Cysharp.Threading.Tasks;");
            sb.AppendLine($"using EmergenceSDK.Internal.Utils;");
            sb.AppendLine($"using EmergenceSDK.Services;");
            sb.AppendLine($"using System.Numerics;");
            sb.AppendLine($"using EmergenceSDK.Types.Responses;");
            sb.AppendLine($"using Newtonsoft.Json;");

            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            // Generate the success callback members
            foreach (JObject functionObj in abiArray)
            {
                string functionName = GetFunctionName(functionObj);
                string stateMutability = GetStateMutability(functionObj);
                bool isReadMethod = IsViewOrPure(stateMutability);
                string returnType = isReadMethod ? GetCSharpReturnType(functionObj) : "void";

                // Generate the success callback member
                GenerateSuccessCallback(sb, functionName, returnType, isReadMethod);
            }

            // Add the contractService field
            AppendLineWithIndent(sb, "private readonly IContractService contractService;", 1);
            AppendLineWithIndent(sb, "private readonly ContractInfo contractInfo;", 1);

            // Generate the constructor
            GenerateConstructor(sb);

            // Iterate through the functions in the ABI
            foreach (JObject functionObj in abiArray)
            {
                string functionName = GetFunctionName(functionObj);
                string stateMutability = GetStateMutability(functionObj);

                bool isRead = IsViewOrPure(stateMutability);

                string returnType = isRead ? GetCSharpReturnType(functionObj) : "void";

                // Generate the private class for parameters
                var generatedClassName = GenerateParameterClass(sb, functionObj);

                GenerateFunctionSignature(sb, functionName, returnType, generatedClassName, isRead);
                GenerateFunctionBody(sb, functionObj);
                sb.AppendLine();
            }

            sb.AppendLine("}");

            // Return the generated class as a string
            return sb.ToString();
        }

        private string GetFunctionName(JObject functionObj) => functionObj["name"].ToString();
        private string GetStateMutability(JObject functionObj) => functionObj["stateMutability"]?.ToString();

        private void GenerateConstructor(StringBuilder sb)
        {
            // Generate the constructor signature
            AppendLineWithIndent(sb, $"public {className}(ContractInfo contractInfoIn)", 1);
            AppendLineWithIndent(sb, "{", 1);

            // Assign the contractService field
            AppendLineWithIndent(sb, "contractService = EmergenceServices.GetService<IContractService>();", 2);
            AppendLineWithIndent(sb, "contractInfo = contractInfoIn;", 2);

            // Close the constructor
            AppendLineWithIndent(sb, "}", 1);
        }

        private void GenerateFunctionSignature(StringBuilder sb, string functionName, string returnType, string className, bool isReadMethod)
        {
            // Generate the base function signature
            string functionSignature = $"public async UniTask {functionName}({className} parameters{(isReadMethod ? "" : ", string localAccountName, string gasPrice, string value")})";

            // Append the modified function signature to the StringBuilder
            AppendLineWithIndent(sb, functionSignature, 1);
        }

        private string GenerateParameterClass(StringBuilder sb, JObject functionObj)
        {
            string generatedClassName = $"{GetFunctionName(functionObj)}Parameters";

            AppendLineWithIndent(sb, $"public class {generatedClassName}", 1);
            AppendLineWithIndent(sb, "{", 1);

            JArray inputsArray = (JArray)functionObj["inputs"];

            for (int i = 0; i < inputsArray.Count; i++)
            {
                JObject inputObj = (JObject)inputsArray[i];
                string paramName = inputObj["name"].ToString();
                string sanitizedParamName = string.IsNullOrEmpty(paramName) ? $"param{i}" : paramName;
                string paramType = GetCSharpType(inputObj["type"].ToString());

                AppendLineWithIndent(sb, $"public {paramType} {sanitizedParamName} {{ get; set; }}", 2);
            }

            AppendLineWithIndent(sb, "}", 1);

            return generatedClassName;
        }

        private void GenerateFunctionBody(StringBuilder sb, JObject functionObj)
        {
            AppendLineWithIndent(sb, "{", 1);

            string className = $"{GetFunctionName(functionObj)}Parameters";

            // Generate the code to deserialize parameters into a JSON string
            AppendLineWithIndent(sb, $"string body = JsonConvert.SerializeObject(parameters);", 2);

            // Check state mutability and call the appropriate method
            if (IsViewOrPure(GetStateMutability(functionObj)))
            {
                GenerateReadMethodCall(sb, functionObj);
            }
            else
            {
                GenerateWriteMethodCall(sb, functionObj);
            }

            AppendLineWithIndent(sb, "}", 1);
        }

        private void GenerateReadMethodCall(StringBuilder sb, JObject functionObj)
        {
            string returnType = GetCSharpReturnType(functionObj);
            // Generate the API call to ReadMethod with contractService prefix
            AppendLineWithIndent(sb, $"await contractService.ReadMethod<BaseResponse<{returnType}>>(contractInfo, body, {GetFunctionName(functionObj)}Success, EmergenceLogger.LogError);", 2);
        }

        private void GenerateWriteMethodCall(StringBuilder sb, JObject functionObj)
        {
            // Generate the API call to WriteMethod with contractService prefix
            AppendLineWithIndent(sb, $"await contractService.WriteMethod<BaseResponse<string>>(contractInfo, localAccountName, gasPrice, value, body, {GetFunctionName(functionObj)}Success, EmergenceLogger.LogError);", 2);
        }

        private void GenerateSuccessCallback(StringBuilder sb, string functionName, string returnType, bool isReadMethod)
        {
            string callbackName = $"{functionName}Success";
            string delegateType = isReadMethod ? $"ReadMethodSuccess<BaseResponse<{returnType}>>" : "WriteMethodSuccess<BaseResponse<string>>";

            // Generate the success callback member
            string callbackSignature = $"public event {delegateType} {callbackName};";
            AppendLineWithIndent(sb, callbackSignature, 1);
        }


        private void AppendLineWithIndent(StringBuilder sb, string line, int indentLevel = 0) => sb.AppendLine($"{new string(' ', indentLevel * Indent.Length)}{line}");


        private bool IsViewOrPure(string stateMutability) => stateMutability == "view" || stateMutability == "pure";

        // Helper function to determine the C# return type based on the ABI function
        private string GetCSharpReturnType(JObject functionObj)
        {
            string type = functionObj["outputs"].First?["type"].ToString();
            return GetCSharpType(type);
        }

        // Helper function to map ABI data types to C# data types
        private string GetCSharpType(string abiType)
        {
            switch (abiType)
            {
                case "address":
                    return "string";
                case "bool":
                    return "bool";
                case "uint256":
                    return "BigInteger";
                case "string":
                    return "string";
                // Add more mappings for other ABI types as needed
                default:
                    return "object";
            }
        }
    }
}
