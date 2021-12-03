using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HubController.Entities;
using HubController.Repositories;
using HubController.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace HubController
{
    public class MachineAuthorizerEntryPoint
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
        {
            bool ok = false;
            UserAuthDetails userAuthDetails = null;

            // authorization logic here...
            if (input.AuthorizationToken != null && input.AuthorizationToken.Contains("Basic"))
            {
                userAuthDetails = ExtractUserAuthDetails(input);
                ok = await VerifyPassword(userAuthDetails);
            }

            var authContext = new APIGatewayCustomAuthorizerContextOutput
            {
                [Constants.USER_SUBJECT_CLAIM_TYPE] = userAuthDetails.UserId,
                [Constants.HUB_ID_CLAIM_TYPE] = userAuthDetails.HubId
            };
            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = userAuthDetails.UserId,//principal info here...
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Version = "2012-10-17",
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>() {
                      new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                      {
                           Action = new HashSet<string>(){"execute-api:Invoke"},
                           Effect = ok ? "Allow" : "Deny",
                           Resource = new HashSet<string>(){ input.MethodArn } // resource arn here
                      }
                },
                },
                Context = authContext,
            };
        }

        private static UserAuthDetails ExtractUserAuthDetails(APIGatewayCustomAuthorizerRequest input)
        {
            byte[] data = Convert.FromBase64String(input.AuthorizationToken.Replace("Basic ", ""));
            string decodedAuthToken = Encoding.UTF8.GetString(data);
            var usernameHub = decodedAuthToken.Split(":")[0];
            var password = decodedAuthToken.Split(":")[1];
            var userId = usernameHub.Split("@")[0];
            var hubId = usernameHub.Split("@")[1];
            return new UserAuthDetails(userId, hubId, password);
        }

        private static async Task<bool> VerifyPassword(UserAuthDetails userAuthDetails)
        {
            string region = Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.USEast1.SystemName;
            var hubPasswordService = new HubPasswordService(new DynamoHubPasswordRepository(new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region))), new PasswordService());
            return await hubPasswordService.VerifyPassword(userAuthDetails.UserId, Guid.Parse(userAuthDetails.HubId), userAuthDetails.Password);
        }
    }
}