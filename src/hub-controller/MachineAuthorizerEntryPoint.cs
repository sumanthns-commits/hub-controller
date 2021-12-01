using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Collections.Generic;
using System.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace HubController
{
    public class MachineAuthorizerEntryPoint
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public APIGatewayCustomAuthorizerResponse FunctionHandler(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
        {
            LambdaLogger.Log("In auth function");
            bool ok = false;
            LambdaLogger.Log($"Auth token {input.AuthorizationToken}, Header keys: {string.Join(",", input.Headers.Keys)}, Header values: {string.Join(",", input.Headers.Values)}, Method:  {input.MethodArn}");
            // authorization logic here...
            if (input.Headers.ContainsKey("authorization"))
            {
                string authHeader;
                input.Headers.TryGetValue("authorization", out authHeader);
                LambdaLogger.Log($"Auth header {authHeader}");
                if (authHeader.Contains("Basic"))
                {
                    ok = true;
                }
                LambdaLogger.Log($"Ok {ok}");

            }

            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = "some principal Id",//principal info here...
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Version = "2012-10-17",
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>() {
                      new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                      {
                           Action = new HashSet<string>(){"execute-api:Invoke"},
                           Effect = ok ? "Allow" : "Deny",
                           Resource = new HashSet<string>(){ $"arn:aws:execute-api:*" } // resource arn here
                      }
                },
                }
            };
        }
    }
}